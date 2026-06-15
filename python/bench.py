#!/usr/bin/env python3
"""
Python barcode scanning benchmark.

Usage: python bench.py [--qrcode] [--verbose] <image-file-or-directory>...

Each library scans all specified images and reports:
  [name] found [n] barcodes in [count] images in [ms]ms

Notes:
  - pyzxing spawns a Java subprocess per image; expect it to be significantly
	slower than native libraries. Requires java on PATH.
  - qreader downloads a YOLOv8 weights file (~6 MB) on first run.
  - cv2.barcode.BarcodeDetector is only available in OpenCV builds that
	include the barcode contrib module; it is skipped otherwise.
"""

import sys
import os
import time
from pathlib import Path
from typing import Callable

# ── availability checks ──────────────────────────────────────────────────────

def optional_import(import_fn):
    try:
        return import_fn()
    except ImportError:
        return None

_zxingcpp = optional_import(lambda: __import__("zxingcpp"))
_zbarlight = optional_import(lambda: __import__("zbarlight"))
_pyzbar = optional_import(lambda: __import__("pyzbar"))
_qreader = optional_import(lambda: __import__("qreader"))
_pyzxing = optional_import(lambda: __import__("pyzxing"))
_cv2 = optional_import(lambda: __import__("cv2"))

HAS_CV2_BARCODE = (
    _cv2 is not None
    and hasattr(_cv2, "barcode")
    and hasattr(_cv2.barcode, "BarcodeDetector")
)

from PIL import Image

# ── core bench runner ────────────────────────────────────────────────────────

def bench(name: str, files: list, fn: Callable, verbose: bool = False) -> None:
	name = f"{name:<16}"
	print(f"{name} ", end="", flush=True)
	start = time.perf_counter()
	n = 0
	results = []
	for i, filepath in enumerate(files):
		print(f"\r\033[K{name} {i+1} of {len(files)}", end="", flush=True)
		try:
			image_results = fn(filepath)
			n += len(image_results)
			if verbose:
				results.extend(image_results)
		except Exception as e:
			print(f"\n  FAILED: {filepath}: {e}", flush=True)
	elapsed = (time.perf_counter() - start)
	time_str = f"{elapsed * 1000:5.0f}ms" if len(files) == 1 else f"{elapsed:5.2f}s"
	print(f"\r\033[K{name} found {n:4} barcodes in {len(files)} images in {time_str}")
	if verbose and results:
		results.sort(key=lambda r: r[1])
		for fmt, text in results:
			print(f"  {fmt:<16}: {text}")
		print("")

# ── library functions ────────────────────────────────────────────────────────

def prime_cache_fn() -> Callable:
	def fn(filepath: str) -> list[tuple[str, str]]:
		with Image.open(filepath) as img:
			img.load()
		return []
	return fn


def zxingcpp_fn(qrcode_only: bool) -> Callable:
	def fn(filepath: str) -> list[tuple[str, str]]:
		with Image.open(filepath) as img:
			results = _zxingcpp.read_barcodes(img, formats=_zxingcpp.BarcodeFormat.QRCode if qrcode_only else _zxingcpp.BarcodeFormat.AllReadable)
			return [(str(result.format), str(result.text)) for result in results]
	return fn


def pyzbar_fn(qrcode_only: bool) -> Callable:
	def fn(filepath: str) -> list[tuple[str, str]]:
		with Image.open(filepath) as img:
			results = _pyzbar.pyzbar.decode(img, symbols=[_pyzbar.pyzbar.ZBarSymbol.QRCODE] if qrcode_only else None)
			return [(result.type, str(result.data)) for result in results]
	return fn


def zbarlight_fn(qrcode_only: bool) -> Callable:
	def fn(filepath: str) -> list[tuple[str, str]]:
		types = ['qrcode'] if qrcode_only else ['ean8', 'upce', 'isbn10', 'upca', 'isbn13', 'ean13', 'i25', 'pdf417', 'qrcode', 'code128', 'code39'] # , 'code93', 'databar', 'databar-exp', 'codabar']
		with Image.open(filepath) as img:
			results = _zbarlight.scan_codes(types, img) or []
			return [('?', str(result)) for result in results]
	return fn


def opencv_qr_fn(qrcode_only: bool) -> Callable:
	detector = _cv2.QRCodeDetector()
	def fn(filepath: str) -> list[tuple[str, str]]:
		img = _cv2.imread(filepath)
		ok, texts, *_ = detector.detectAndDecodeMulti(img)
		return [('QRCode', text) for text in (texts or []) if text]
	return fn


def opencv_barcode_fn(qrcode_only: bool) -> Callable:
	detector = _cv2.barcode.BarcodeDetector()
	def fn(filepath: str) -> list[tuple[str, str]]:
		ok, decoded_info, *_ = detector.detectAndDecodeMulti(_cv2.imread(filepath))
		return [('?', info) for info in (decoded_info or []) if info]
	return fn


qreader = _qreader.QReader() if _qreader is not None else None
def qreader_fn(qrcode_only: bool) -> Callable:
	def fn(filepath: str) -> list[tuple[str, str]]:
		results = qreader.detect_and_decode(image = _cv2.imread(filepath), is_bgr = True)
		return [('QRCode', result) for result in results if result is not None]
	return fn


pyzxingreader = _pyzxing.BarCodeReader() if _pyzxing is not None else None
def pyzxing_fn(qrcode_only: bool) -> Callable:
	def fn(filepath: str) -> list[tuple[str, str]]:
		results = pyzxingreader.decode(filepath) or []
		return [
			(str(result.get('format')),result.get('parsed') or '')
			for result in results
			if result.get('parsed') is not None and (not qrcode_only or result.get('format') == b'QR_CODE')
		]
	return fn

# ── main ─────────────────────────────────────────────────────────────────────

def main() -> None:
	IMAGE_EXTENSIONS = {'.png', '.jpg', '.jpeg'}
	qrcode_only = False
	verbose = False
	files = []

	for arg in sys.argv[1:]:
		if arg == '--qrcode':
			qrcode_only = True
		elif arg == '--verbose':
			verbose = True
		elif os.path.isdir(arg):
			for path in sorted(Path(arg).rglob('*')):
				if path.suffix.lower() in IMAGE_EXTENSIONS:
					files.append(str(path))
		else:
			files.append(arg)

	if not files:
		print("Usage: python bench.py [--qrcode] [--verbose] <image-file-or-directory>...",
			  file=sys.stderr)
		sys.exit(1)

	bench("PrimeFileCache", files, prime_cache_fn(), verbose)

	if _zxingcpp is not None:
		bench("zxingcpp", files, zxingcpp_fn(qrcode_only), verbose)

	if _pyzbar is not None:
		bench("pyzbar", files, pyzbar_fn(qrcode_only), verbose)

	if _zbarlight is not None:
		bench("zbarlight", files, zbarlight_fn(qrcode_only), verbose)

	if _cv2 is not None:
		bench("opencv-qr", files, opencv_qr_fn(qrcode_only), verbose)
		if HAS_CV2_BARCODE and not qrcode_only:
			bench("opencv-barcode", files, opencv_barcode_fn(qrcode_only), verbose)

	if _qreader is not None:
		bench("qreader (YOLOv8)", files, qreader_fn(qrcode_only), verbose)

	if _pyzxing is not None and len(files) == 1:
		bench("pyzxing (Java)", files, pyzxing_fn(qrcode_only), verbose)


if __name__ == "__main__":
	main()
