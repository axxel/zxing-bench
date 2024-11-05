# zxing-bench

This is a little experiment related to my latest effort of bringing
[zxing-cpp](https://github.com/zxing-cpp/zxing-cpp) to the .NET community.

Looking at nuget.org for barcode reader libraries there are a bunch of options of which I took two of the most popular:
 * [Dynamsoft](https://www.nuget.org/packages/Dynamsoft.DotNet.Barcode) (a .NET wrapper around the commercial Dynamsoft c++ library)
 * [ZXing.Net](https://www.nuget.org/packages/ZXing.Net) (an open source C# port of the ZXing java library)
 * [ZXingCpp](https://www.nuget.org/packages/ZXingCpp) (a .NET wrapper around the open source zxing-cpp c++ library)

 To run a simple benchmark comparison execute
 ```sh
 dotnet run -- <image-file-or-directory, ...>
 ```

When run for [this image](https://github.com/Dynamsoft/barcode-reader-dotnet-samples/blob/main/images/AllSupportedBarcodeTypes.png)
results are as follows on my 4 year old mobile Core i9 CPU:
```
ZXing.Net   found   9 barcodes in 1 images in 2935ms
Dynamsoft   found  14 barcodes in 1 images in  217ms
ZXingCpp    found  16 barcodes in 1 images in   81ms
```

Executing with the 'false positive' [test image](https://github.com/zxing-cpp/zxing-cpp/blob/master/test/samples/falsepositives-1/16.png)
results in:
```
ZXing.Net   found   1 barcodes in 1 images in  987ms
Dynamsoft   found   0 barcodes in 1 images in  230ms
ZXingCpp    found   0 barcodes in 1 images in   25ms
```

Letting it run through all the [test images](https://github.com/zxing-cpp/zxing-cpp/blob/master/test/samples) we get:
```
ZXing.Net   found 890 barcodes in 1087 images in 50251ms
Dynamsoft   found 964 barcodes in 1087 images in 26801ms
ZXingCpp    found 962 barcodes in 1087 images in  5555ms
```

Doing the same with the `--single` mode, which looks for at most one barcode per image, results in:
```
ZXing.Net   found 858 barcodes in 1087 images in 24586ms
Dynamsoft   found 944 barcodes in 1087 images in 25534ms
ZXingCpp    found 943 barcodes in 1087 images in  2981ms
```

For [this set](https://boofcv.org/index.php?title=Performance:QrCode) of very challenging QRCodes we get:
```
ZXing.Net found 323 barcodes in 537 images in 291s
Dynamsoft found 895 barcodes in 537 images in 105s
ZXingCpp  found 969 barcodes in 537 images in  52s
```

My personal and "biased" conclusion: use `ZXingCpp` ;-)
