# zxing-bench

This is a little experiment related to my latest effort of bringing [zxing-cpp](https://github.com/zxing-cpp/zxing-cpp) to the Rust community.

Looking at crates.io for barcode reader software there seem to be mainly 3 projects that are actively maintained:
 * https://crates.io/crates/rxing (a native Rust port of the ZXing java library)
 * https://crates.io/crates/zbar-rust (a Rust wrapper around the zbar c library)
 * https://crates.io/crates/zxing-cpp (a Rust wrapper around the zxing-cpp c++ library)

 To run a simple benchmark comparison execute
 ```sh
 cargo run --release -- <image-file-name>
 ```

When run for [this image](https://user-images.githubusercontent.com/15202578/170050507-1f10f0ef-82ca-4e14-a2d2-4b288ec54809.png) with the global variable `USE_COMMON_FORMATS` set to `true` to make the runtime comparison fair, this results in the following output on my 4 year old mobile Core i9 CPU:

```
running zxing-cpp...
  ITF         : 00123456
  EAN-13      : 0012345678905
  Codabar     : 012345
  UPC-E       : 01234565
  EAN-8       : 01234565
  EAN-13      : 1234567890128
  Code128     : CODE128
  Code39      : CODE39
  Code93      : CODE93
found 9 codes in 78ms

running rxing...
  ean 13      : 0012345678905
  codabar     : 012345
  upc e       : 01234565
  ean 13      : 1234567890128
  code 128    : CODE128
  code 39     : CODE39
  code 93     : CODE93
found 7 codes in 1860ms

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
found 9 codes in 140ms
```

With `USE_COMMON_FORMATS = false` to detect all supported formats, the timinings change as follows:

```
running zxing-cpp...
found 9 codes in 128ms

running rxing...
found 7 codes in 3023ms

running zbar-rust...
found 9 codes in 126ms
```

Executing with the 'false positive' [test image](https://github.com/zxing-cpp/zxing-cpp/blob/master/test/samples/falsepositives-1/16.png) for the common formats results in:

```
running zxing-cpp...
found 0 codes in 9ms

running rxing...
  upc e       : 19758226
found 1 codes in 93ms

running zbar-rust...
found 0 codes in 34ms
```

My personal and opinionated conclusion: use `zxing-cpp`. ;-)