/*
* Copyright 2024 Axel Waggershauser
*/
// SPDX-License-Identifier: Apache-2.0

struct FmtTxt(String, String);
type Results = Vec<FmtTxt>;

fn test_zxing_cpp(image: &image::GrayImage, limit_formats: bool) -> Results {
    use zxingcpp::BarcodeFormat::*;
    use zxingcpp::*;

    let mut read = read().try_invert(false);

    if limit_formats {
        read.set_formats(Codabar | Code39 | Code93 | Code128 | DataBar | EAN8 | EAN13 | ITF | QRCode | UPCE);
    }

//    let iv = ImageView::from_slice(image.as_ref(), image.width(), image.height(), ImageFormat::Lum).unwrap();

    read.from(image)
        .unwrap()
        .iter()
        .map(|r| FmtTxt(r.format().to_string(), r.text()))
        .collect()
}

fn test_rxing(image: &image::GrayImage, limit_formats: bool) -> Results {
    use rxing::BarcodeFormat::*;
    use rxing::*;
    use std::collections::HashSet;

    let mut hints = DecodingHintDictionary::default();
    // hints.insert(DecodeHintType::TRY_HARDER, DecodeHintValue::TryHarder(true));
    // hints.insert(DecodeHintType::ALSO_INVERTED, DecodeHintValue::AlsoInverted(false));

    if limit_formats {
        hints.insert(
            DecodeHintType::POSSIBLE_FORMATS,
            DecodeHintValue::PossibleFormats(HashSet::from([
                CODABAR, CODE_39, CODE_93, CODE_128, RSS_14, EAN_8, EAN_13, ITF, QR_CODE, UPC_E,
            ])),
        );
    }

    let results = rxing::helpers::detect_multiple_in_luma_with_hints(
        image.clone().into_raw(),
        image.width(),
        image.height(),
        &mut hints,
    )
    .unwrap_or_default();

    results
        .iter()
        .map(|r| FmtTxt(r.getBarcodeFormat().to_string(), r.getText().to_string()))
        .collect()
}

fn test_zbar_rust(image: &image::GrayImage) -> Results {
    use zbar_rust::ZBarImageScanner;
    let (width, height) = image.dimensions();
    let mut scanner = ZBarImageScanner::new();
    let results = scanner
        .scan_y800(image.as_ref(), width, height)
        .unwrap_or_default();
    results
        .iter()
        .map(|r| {
            FmtTxt(
                format!("{:?}", &r.symbol_type),
                std::str::from_utf8(&r.data).unwrap().to_string(),
            )
        })
        .collect()
}

fn bench<F>(f: F, name: &str)
where
    F: Fn() -> Results,
{
    use std::time::Instant;
    let now = Instant::now();
    let mut results = f();
    let elapsed = now.elapsed();
    results.sort_by_key(|r| r.1.clone());

    println!("running {}...", name);
    for r in results.iter() {
        println!("  {:<12}: {}", r.0, r.1);
    }
    println!(
        "{:9} found {:3} codes in {:4}ms\n",
        name,
        results.len(),
        elapsed.as_millis()
    );
}

fn main() {
    let args: Vec<String> = std::env::args().collect();
    let image = image::open(&args[1]).unwrap().into_luma8();
    let limit_formats = args.len() < 3 || args[2] != "--all";

    bench(|| test_rxing(&image, limit_formats), "rxing");
    bench(|| test_zbar_rust(&image), "zbar-rust");
    bench(|| test_zxing_cpp(&image, limit_formats), "zxing-cpp");
}
