# zxing-bench

This is a little experiment related to my latest effort of bringing [zxing-cpp](https://github.com/zxing-cpp/zxing-cpp) to the Rust community.

Looking at crates.io for barcode reader software there seem to be mainly 6 projects that are actively maintained:
 * https://crates.io/crates/rxing (a native Rust port of the ZXing Java and partly the zxing-cpp C++ library)
 * https://crates.io/crates/zbar-rust (a Rust wrapper around the zbar C library)
 * https://crates.io/crates/zedbar (a Rust port of the zbar C library)
 * https://crates.io/crates/bardecoder (a native Rust QR code decoder)
 * https://crates.io/crates/rqrr (a native Rust QR code decoder)
 * https://crates.io/crates/zxing-cpp (a Rust wrapper around the zxing-cpp C++ library)

 To run a simple benchmark comparison execute
 ```sh
 cargo run --release -- <image-file-or-directory>... [--common] [--verbose]
 ```

When run for [this image](https://user-images.githubusercontent.com/15202578/170050507-1f10f0ef-82ca-4e14-a2d2-4b288ec54809.png) with the `--common` flag to limit the detection to the set of formats that all libraries support to make the runtime comparison fair (Note: not quite correct anymore as `rqrr` and `bardecoder` only support QRCodes), this results in the following output on my Apple M4 Pro:

```
rxing      found    9 barcodes in 1 images in   432 ms
zbar-rust  found    9 barcodes in 1 images in    38 ms
zedbar     found    9 barcodes in 1 images in    44 ms
bardecoder found    0 barcodes in 1 images in    63 ms
rqrr       found    0 barcodes in 1 images in    37 ms
zxing-cpp  found    9 barcodes in 1 images in    17 ms
```

Detecting all supported formats from [this image](https://github.com/Dynamsoft/barcode-reader-dotnet-samples/blob/b26e38efe1db2bb6ba11ead320dea07471d988a3/Images/GeneralBarcodes.png) results in:

```
rxing      found   15 barcodes in 1 images in   338 ms
zbar-rust  found   11 barcodes in 1 images in    20 ms
zedbar     found   11 barcodes in 1 images in    19 ms
bardecoder found    0 barcodes in 1 images in    16 ms
rqrr       found    1 barcodes in 1 images in    10 ms
zxing-cpp  found   16 barcodes in 1 images in    19 ms
```

Executing with `--common` with the 'none' (meaning there are no barcodes in them) [test images](https://github.com/zxing-cpp/zxing-cpp/blob/master/test/samples/none-2/) results in:

```
rxing      found    8 barcodes in 25 images in  0.58 s
zbar-rust  found    0 barcodes in 25 images in  0.16 s
zedbar     found    0 barcodes in 25 images in  0.20 s
bardecoder found    0 barcodes in 25 images in  0.05 s
rqrr       found    0 barcodes in 25 images in  0.03 s
zxing-cpp  found    0 barcodes in 25 images in  0.05 s
```

Executing with `--common` with the 'QRCode' [test images](https://github.com/zxing-cpp/zxing-cpp/blob/master/test/samples/qrcode-2/) results in:

```
rxing      found   34 barcodes in 38 images in  0.07 s
zbar-rust  found   20 barcodes in 38 images in  0.07 s
zedbar     found   22 barcodes in 38 images in  0.08 s
bardecoder found   11 barcodes in 38 images in  0.03 s
rqrr       found   11 barcodes in 38 images in  0.05 s
zxing-cpp  found   39 barcodes in 38 images in  0.03 s
```

For [this set](https://boofcv.org/index.php?title=Performance:QrCode) of very challenging QRCodes with `--common` again we get:

```
rxing      found  980 barcodes in 542 images in 28.97 s
zbar-rust  found  756 barcodes in 542 images in 41.11 s
zedbar     found  700 barcodes in 542 images in 39.54 s
bardecoder found    ? barcodes in 138 images ... and crashed with index out of bounds
rqrr       found    ? barcodes in 184 images ... and crashed with assertion failed
zxing-cpp  found 1150 barcodes in 542 images in  5.70 s
```

My personal (and biased) conclusion: use `zxing-cpp`. ;-)
