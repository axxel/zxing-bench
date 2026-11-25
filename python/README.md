# zxing-bench

This is a little experiment related to my efforts of bringing [zxing-cpp](https://github.com/zxing-cpp/zxing-cpp) to the Python community.

Looking at pypi.org for barcode reader software there seem to be mainly the following projects that are actively maintained: zxing-cpp, pyrxing, pyzbar, zbarlight, pyzxing, qreader, opencv-python, qrcode-barcode-scanner.

 To run a simple benchmark comparison execute
 ```sh
 pip install -r requirements.txt
 python bench.py <image-file-or-directory> [--qrcode]
 ```

The optional `opencv-wechat-qr` scanner requires OpenCV 4.x's `wechat_qrcode`
module and four model files. OpenCV 5 uses different ONNX models and cannot
load these Caffe models. The requirements include both OpenCV packages because
qreader depends on `opencv-python`. Since they share the `cv2` namespace,
reinstall the contrib build after the requirements to ensure it is active:

```sh
pip install --force-reinstall --no-deps 'opencv-contrib-python<5'
```

From the `python` directory, download the models once:

```sh
mkdir -p wechat_qrcode_models
for model in detect.prototxt detect.caffemodel sr.prototxt sr.caffemodel; do
  curl -fL "https://raw.githubusercontent.com/WeChatCV/opencv_3rdparty/wechat_qrcode/$model" -o "wechat_qrcode_models/$model"
done
```

The benchmark uses this directory relative to `bench.py`, regardless of the
current working directory. It reports when the module or models are missing,
or the OpenCV version is incompatible.
Like `opencv-qr`, the WeChat scanner runs with or without `--qrcode` and reads
only QR codes.

Detecting all supported formats from [this image](https://github.com/Dynamsoft/barcode-reader-dotnet-samples/blob/b26e38efe1db2bb6ba11ead320dea07471d988a3/Images/GeneralBarcodes.png) results in:

```
zxingcpp         found   16 barcodes in 1 images in    26ms
pyrxing          found    0 barcodes in 1 images in    22ms FAILED: `Code 39 Extended` is not a supported barcode format
pyzbar           found   11 barcodes in 1 images in    41ms
zbarlight        found    9 barcodes in 1 images in    20ms
opencv-qr        found    1 barcodes in 1 images in    81ms
opencv-wechat-qr found    1 barcodes in 1 images in    43ms
opencv-barcode   found    3 barcodes in 1 images in    25ms
qrcode-ai-com    found    1 barcodes in 1 images in   100ms
qreader (YOLOv8) found    1 barcodes in 1 images in   608ms
pyzxing (Java)   found   12 barcodes in 1 images in  2552ms
```

Executing with the 'none' (meaning there are no barcodes in them) [test images](https://github.com/zxing-cpp/zxing-cpp/blob/master/test/samples/none-2/) results in:

```
zxingcpp         found    0 barcodes in 25 images in  0.17s
pyrxing          found    0 barcodes in 25 images in  0.21s
pyzbar           found    0 barcodes in 25 images in  0.21s
zbarlight        found    0 barcodes in 25 images in  0.18s
opencv-qr        found    0 barcodes in 25 images in  0.17s
opencv-wechat-qr found    0 barcodes in 25 images in  0.37s
opencv-barcode   found    0 barcodes in 25 images in  0.09s
qrcode-ai-com    found   10 barcodes in 25 images in 16.88s
qreader (YOLOv8) found    0 barcodes in 25 images in  3.29s
```

For [this set](https://boofcv.org/index.php?title=Performance:QrCode) of very challenging QRCodes with the `--qrcode` flag passed we get:

```
zxingcpp         found 1119 barcodes in 542 images in  10.26s
pyrxing          found 1121 barcodes in 542 images in   8.04s
pyzbar           found  746 barcodes in 542 images in  36.00s
zbarlight        found  746 barcodes in 542 images in  36.18s
opencv-qr        found  702 barcodes in 542 images in 143.08s
opencv-wechat-qr found  513 barcodes in 542 images in  46.24s
```

My personal and opinionated conclusion: use `zxing-cpp`. ;-)
