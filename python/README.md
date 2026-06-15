# zxing-bench

This is a little experiment related to my efforts of bringing [zxing-cpp](https://github.com/zxing-cpp/zxing-cpp) to the Python community.

Looking at pypi.org for barcode reader software there seem to be mainly the following projects that are actively maintained: zxing-cpp, pyzbar, zbarlight, pyzxing, qreader, opencv-python.

 To run a simple benchmark comparison execute
 ```sh
 pip install -r requirements.txt
 python bench.py <image-file-or-directory> [--qrcode]
 ```

Detecting all supported formats from [this image](https://github.com/Dynamsoft/barcode-reader-dotnet-samples/blob/b26e38efe1db2bb6ba11ead320dea07471d988a3/Images/GeneralBarcodes.png) results in:

```
zxingcpp         found   16 barcodes in 1 images in    24ms
pyzbar           found   11 barcodes in 1 images in    42ms
zbarlight        found    9 barcodes in 1 images in    21ms
opencv-qr        found    1 barcodes in 1 images in    68ms
opencv-barcode   found    3 barcodes in 1 images in    24ms
qreader (YOLOv8) found    1 barcodes in 1 images in   487ms
pyzxing (Java)   found   12 barcodes in 1 images in  1884ms
```

Executing with the 'false positive' (meaning there are no barcodes in them) [test images](https://github.com/zxing-cpp/zxing-cpp/blob/master/test/samples/falsepositives-2/) results in:

```
zxingcpp         found    0 barcodes in 25 images in  0.16s
pyzbar           found    1 barcodes in 25 images in  0.20s
zbarlight        found    1 barcodes in 25 images in  0.17s
opencv-qr        found    0 barcodes in 25 images in  0.16s
opencv-barcode   found    1 barcodes in 25 images in  0.09s
qreader (YOLOv8) found    0 barcodes in 25 images in  3.15s
```

For [this set](https://boofcv.org/index.php?title=Performance:QrCode) of very challenging QRCodes with the `--qrcode` flag passed we get:

```
zxingcpp         found 1113 barcodes in 540 images in  11.99s
pyzbar           found  387 barcodes in 540 images in  35.74s
zbarlight        found  387 barcodes in 540 images in  35.17s
opencv-qr        found  702 barcodes in 540 images in 140.36s
qreader (YOLOv8) found 1048 barcodes in 540 images in 261.93s
```

My personal and opinionated conclusion: use `zxing-cpp`. ;-)
