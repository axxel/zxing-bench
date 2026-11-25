# zxing-bench

This is a little experiment related to my latest effort of bringing
[zxing-cpp](https://github.com/zxing-cpp/zxing-cpp) to the .NET community.

Looking at nuget.org for barcode reader libraries there are a bunch of options of which I took two of the most popular:
 * [Dynamsoft](https://www.nuget.org/packages/Dynamsoft.DotNet.BarcodeReader.Bundle/) (a .NET wrapper around the commercial Dynamsoft c++ library (not available for free or for macOS))
 * [FeatherQR](https://www.nuget.org/packages/FeatherQR.SkiaSharp/) (an open source .NET QR, Micro QR and rMQR decoder)
 * [ZXing.Net](https://www.nuget.org/packages/ZXing.Net) (an open source C# port of the ZXing java library)
 * [ZXingCpp](https://www.nuget.org/packages/ZXingCpp) (a .NET wrapper around the open source zxing-cpp c++ library)

 To run a simple benchmark comparison execute
 ```sh
 dotnet run -- <image-file-or-directory, ...>
 ```

Detecting all supported formats from [this image](https://github.com/Dynamsoft/barcode-reader-dotnet-samples/blob/b26e38efe1db2bb6ba11ead320dea07471d988a3/Images/GeneralBarcodes.png) results in the following output on my Apple M4 Pro:
```
ZXing.Net        found   11 barcodes in 1 images in   864ms
FeatherQR        found    2 barcodes in 1 images in   113ms
ZXingCpp         found   16 barcodes in 1 images in    20ms
```

Executing with the 'none' [test images](https://github.com/zxing-cpp/zxing-cpp/blob/master/test/samples/none-2) that have no barcodes in them results in:
```
ZXing.Net        found    5 barcodes in 25 images in  3.04s
FeatherQR        found    0 barcodes in 25 images in  0.32s
ZXingCpp         found    0 barcodes in 25 images in  0.16s
```

Letting it run through all the [test images](https://github.com/zxing-cpp/zxing-cpp/blob/master/test/samples) we get:
```
ZXing.Net        found  641 barcodes in 914 images in 12.44s
FeatherQR        found  188 barcodes in 914 images in  1.22s
ZXingCpp         found  850 barcodes in 914 images in  1.36s
```

For [this set](https://boofcv.org/index.php?title=Performance:QrCode) of very challenging QRCodes with the `--qrcode` option to only look for QRCode symbols the results are:
```
ZXing.Net        found  703 barcodes in 542 images in 30.68s
FeatherQR        found 1205 barcodes in 542 images in 10.99s
ZXingCpp         found 1119 barcodes in 542 images in  6.99s
```

My personal and "biased" conclusion: use `ZXingCpp` ;-)
