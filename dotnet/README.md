# zxing-bench

This is a little experiment related to my latest effort of bringing
[zxing-cpp](https://github.com/zxing-cpp/zxing-cpp) to the .NET community.

Looking at nuget.org for barcode reader libraries there are a bunch of options of which I took two of the most popular:
 * https://www.nuget.org/packages/Dynamsoft.DotNet.Barcode (a .NET wrapper around the commercial Dynamsoft c++ library)
 * https://www.nuget.org/packages/ZXing.Net (an open source C# port of the ZXing java library)
 * https://www.nuget.org/packages/ZXingCpp/ (a .NET wrapper around the open source zxing-cpp c++ library)

 To run a simple benchmark comparison execute
 ```sh
 dotnet run -- <image-file-name>
 ```

When run for [this image](https://github.com/Dynamsoft/barcode-reader-dotnet-samples/blob/main/images/AllSupportedBarcodeTypes.png)
results are as follows on my 4 year old mobile Core i9 CPU:

```
  QR_CODE : www.dynamsoft.com
  CODE_93 : CODE93
  CODE_39 : CODE39
  UPC_E : 19107462
  CODE_128 : CODE128
  CODABAR : 012345
  UPC_A : 012345678905
  UPC_E : 01234565
  EAN_13 : 1234567890128
ZXing.Net found 9 barcodes in 3303ms

  CODE_128 : CODE128
  CODE_93 : CODE93
  AZTEC : Dynamsoft
  DATAMATRIX : www.dynamsoft.com
  GS1 Databar Stacked Omnidirectional : 01230456078905
  PDF417 : www.dynamsoft.com
  QR_CODE : www.dynamsoft.com
  EAN_13 : 1234567890128
  ITF : 00123456
  UPC_A : 012345678905
  CODABAR : 012345
  CODE_39_EXTENDED : CODE39
  EAN_8 : 01234565
  UPC_E : 01234565
Dynamsoft found 14 barcodes in 207ms

  Code39 : CODE39
  Code128 : CODE128
  DataBar : 01230456078905
  Codabar : C012345D
  EAN8 : 01234565
  ITF : 00123456
  Code93 : CODE93
  UPCA : 012345678905
  EAN13 : 1234567890128
  UPCE : 01234565
  PDF417 : www.dynamsoft.com
  DataMatrix : www.dynamsoft.com
  QRCode : www.dynamsoft.com
  Aztec : Dynamsoft
ZXingCpp  found 14 barcodes in 172ms
```

Executing with the 'false positive' [test image](https://github.com/zxing-cpp/zxing-cpp/blob/master/test/samples/falsepositives-1/16.png)
results in:

```
ZXing.Net found 0 barcodes in 959ms

Dynamsoft found 0 barcodes in 238ms

ZXingCpp  found 0 barcodes in 31ms
```

My personal and opinionated conclusion: use `ZXingCpp`. ;-)