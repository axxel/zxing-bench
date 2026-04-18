# zxing-bench

This is a little experiment related to my latest effort of bringing [zxing-cpp](https://github.com/zxing-cpp/zxing-cpp) to the Rust community.

Looking at crates.io for barcode reader software there seem to be mainly 3 projects that are actively maintained:
 * https://crates.io/crates/rxing (a native Rust port of the ZXing java library)
 * https://crates.io/crates/zbar-rust (a Rust wrapper around the zbar c library)
 * https://crates.io/crates/zedbar (a Rust port of the zbar c library)
 * https://crates.io/crates/zxing-cpp (a Rust wrapper around the zxing-cpp c++ library)

 To run a simple benchmark comparison execute
 ```sh
 cargo run --release -- <image-file-name> [--all]
 ```

When run for [this image](https://user-images.githubusercontent.com/15202578/170050507-1f10f0ef-82ca-4e14-a2d2-4b288ec54809.png) without the `--all` flag to make the runtime comparison fair, this results in the following output on my Apple M4 Pro:

```
running rxing...
  itf         : 00123456
  ean 13      : 0012345678905
  codabar     : 012345
  ean 8       : 01234565
  upc e       : 01234565
  ean 13      : 1234567890128
  code 128    : CODE128
  code 39     : CODE39
  code 93     : CODE93
rxing     found   9 codes in 1209ms

running zbar-rust...
  ZBarEAN13   : 0012345000065
  ZBarI25     : 00123456
  ZBarEAN13   : 0012345678905
  ZBarEAN8    : 01234565
  ZBarEAN13   : 1234567890128
  ZBarCodeBar : C012345D
  ZBarCode128 : CODE128
  ZBarCode39  : CODE39
  ZBarCode93  : CODE93
zbar-rust found   9 codes in   39ms

running zedbar...
  Ean13       : 0012345000065
  I25         : 00123456
  Ean13       : 0012345678905
  Ean13       : 1234567890128
  Codabar     : C012345D
  Code39      : CODE39
  Code93      : CODE93
zedbar    found   7 codes in   43ms

running zxing-cpp...
  UPC-E       : 0012345000065
  ITF         : 00123456
  EAN-13      : 0012345678905
  EAN-8       : 01234565
  EAN-13      : 1234567890128
  Codabar     : C012345D
  Code 128    : CODE128
  Code 39     : CODE39
  Code 93     : CODE93
zxing-cpp found   9 codes in   15ms

```

Passing `--all` to detect all supported formats from [this image](https://github.com/Dynamsoft/barcode-reader-dotnet-samples/blob/b26e38efe1db2bb6ba11ead320dea07471d988a3/Images/GeneralBarcodes.png) results in:

```
rxing     found  15 codes in  537ms
zbar-rust found  11 codes in   17ms
zedbar    found   9 codes in   19ms
zxing-cpp found  15 codes in   11ms
```

Executing with the 'false positive' [test image](https://github.com/zxing-cpp/zxing-cpp/blob/master/test/samples/falsepositives-1/16.png) for the common formats results in:

```
rxing     found   0 codes in   39ms
zbar-rust found   0 codes in   16ms
zedbar    found   0 codes in   19ms
zxing-cpp found   0 codes in    5ms
```

My personal and opinionated conclusion: use `zxing-cpp`. ;-)
