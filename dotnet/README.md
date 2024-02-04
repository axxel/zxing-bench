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
ZXing.Net found 9 barcodes in 3164ms
Dynamsoft found 14 barcodes in 216ms
ZXingCpp  found 14 barcodes in 79ms
```

Executing with the 'false positive' [test image](https://github.com/zxing-cpp/zxing-cpp/blob/master/test/samples/falsepositives-1/16.png)
results in:
```
ZXing.Net found 1 barcodes in 1208ms
Dynamsoft found 0 barcodes in  239ms
ZXingCpp  found 0 barcodes in   25ms
```

Letting it run through all the [test images](https://github.com/zxing-cpp/zxing-cpp/blob/master/test/samples) we get:
```
ZXing.Net found 890 barcodes in 49895ms
Dynamsoft found 960 barcodes in 27906ms
ZXingCpp  found 953 barcodes in  4962ms
```

Doing the same with the `--single` mode, which looks for at most one barcode per image, results in:
```
ZXing.Net found 858 barcodes in 24537ms
Dynamsoft found 940 barcodes in 25714ms
ZXingCpp  found 928 barcodes in  2702ms
```

For [this set](https://boofcv.org/index.php?title=Performance:QrCode) of very challenging QRCodes we get:
```
ZXing.Net found 323 barcodes in 291s
Dynamsoft found 895 barcodes in 105s
ZXingCpp  found 975 barcodes in  50s
```

My personal and "biased" conclusion: use `ZXingCpp` ;-)
