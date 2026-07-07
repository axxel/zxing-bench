/*
* Copyright 2024 Axel Waggershauser
*/
// SPDX-License-Identifier: Apache-2.0

struct FmtTxt(String, String);
type Results = Vec<FmtTxt>;

const IMAGE_EXTENSIONS: &[&str] = &["png", "jpg", "jpeg", "webp"];

fn test_zxing_cpp(image: &image::GrayImage, limit_formats: bool) -> Results {
    use zxingcpp::BarcodeFormat::*;
    use zxingcpp::*;

    let mut read = read().try_invert(false);

    if limit_formats {
        read.set_formats([Codabar, Code39, Code93, Code128, DataBarOmni, DataBarStkOmni, DataBarExp, DataBarExpStk, EAN8, EAN13, ITF, QRCodeModel2, UPCE]);
    }

//    let iv = ImageView::from_slice(image.as_ref(), image.width(), image.height(), ImageFormat::Lum).unwrap();

    read.from(image)
        .unwrap()
        .iter()
        .map(|r| FmtTxt(format!("{:?}", r.format()), r.text()))
        .collect()
}

fn test_rxing(image: &image::GrayImage, limit_formats: bool) -> Results {
    use rxing::BarcodeFormat::*;
    use rxing::*;
    use std::collections::HashSet;

    let mut hints = DecodeHints::default();
    if limit_formats {
        hints.PossibleFormats = Some(HashSet::from([
            CODABAR, CODE_39, CODE_93, CODE_128, RSS_14, RSS_EXPANDED, EAN_8, EAN_13, ITF, QR_CODE, UPC_E,
        ]));
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
        .map(|r| FmtTxt(format!("{:?}", r.getBarcodeFormat()), r.getText().to_string()))
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

fn test_zedbar(image: &image::GrayImage, limit_formats: bool) -> Results {
    use zedbar::{DecoderConfig, Image, Scanner, config::*};
    let (width, height) = image.dimensions();
    let mut img = Image::from_gray(image.as_raw(), width, height).unwrap();
    let mut cfg = DecoderConfig::all().retry_undecoded_regions(false);
    if limit_formats {
        cfg = cfg.disable(SqCode);
    }
    let mut scanner = Scanner::with_config(cfg);
    let results = scanner.scan(&mut img);
    results
        .iter()
        .map(|s| {
            FmtTxt(
                format!("{:?}", s.symbol_type()),
                s.data_string().unwrap_or_default().to_string(),
            )
        })
        .collect()
}

fn test_bardecoder(image: &image::GrayImage) -> Results {
    let image24 = image24::GrayImage::from_raw(image.width(), image.height(), image.as_raw().clone())
        .expect("invalid gray image buffer shape");
    let dyn_image24 = image24::DynamicImage::ImageLuma8(image24);
    let decoder = bardecoder::default_decoder();
    decoder
        .decode(&dyn_image24)
        .into_iter()
        .filter_map(|r| r.ok())
        .map(|text| FmtTxt("QRCode".to_string(), text))
        .collect()
}

fn test_rqrr(image: &image::GrayImage) -> Results {
    let mut prepared = rqrr::PreparedImage::prepare(image.clone());
    prepared
        .detect_grids()
        .into_iter()
        .filter_map(|g| g.decode().ok())
        .map(|(_, text)| FmtTxt("QRCode".to_string(), text))
        .collect()
}

fn bench<F>(f: F, name: &str, files: &[String], verbose: bool)
where
    F: Fn(&image::GrayImage) -> Results,
{
    let display_name = format!("{:<10}", name);
    print!("{} ", display_name);
    use std::io::Write;
    std::io::stdout().flush().unwrap();

    let start = std::time::Instant::now();
    let mut total_count = 0usize;
    let mut results: Results = Vec::new();

    for (i, file) in files.iter().enumerate() {
        print!("\r\x1b[K{} {} of {}", display_name, i + 1, files.len());
        std::io::stdout().flush().unwrap();
        match image::open(file) {
            Ok(img) => {
                let mut image_results = f(&img.into_luma8());
                total_count += image_results.len();
                if verbose {
                    results.append(&mut image_results);
                }
            }
            Err(err) => {
                eprintln!("  FAILED: {}: {}", file, err);
            }
        }
    }

    let elapsed = start.elapsed();
    let time_str = if files.len() == 1 {
        format!("{:5} ms", elapsed.as_millis())
    } else {
        format!("{:5.2} s", elapsed.as_secs_f64())
    };

    println!(
        "\r\x1b[K{} found {:4} barcodes in {} images in {}",
        display_name,
        total_count,
        files.len(),
        time_str,
    );

    if verbose {
        results.sort_by_key(|r| r.1.clone());
        for r in &results {
            println!("  {:<14}: {}", r.0, r.1);
        }
        println!("");
    }
}

fn has_supported_extension(path: &std::path::Path) -> bool {
    path.extension()
        .and_then(|ext| ext.to_str())
        .map(|ext| IMAGE_EXTENSIONS.iter().any(|candidate| ext.eq_ignore_ascii_case(candidate)))
        .unwrap_or(false)
}

fn collect_images(path: &std::path::Path, files: &mut Vec<String>) {
    if path.is_file() && has_supported_extension(path) {
        files.push(path.to_string_lossy().to_string());
    } else if let Ok(entries) = std::fs::read_dir(path) {
        for entry in entries.flatten() {
            collect_images(&entry.path(), files);
        }
    }
}

fn main() {
    let mut files = Vec::new();
    let mut limit_formats = false;
    let mut verbose = false;

    for arg in std::env::args().skip(1) {
        match arg.as_str() {
            "--common" => limit_formats = true,
            "--verbose" => verbose = true,
            _ => collect_images(std::path::Path::new(&arg), &mut files),
        }
    }

    if files.is_empty() {
        eprintln!("Usage: cargo run --release -- <image-file-or-directory>... [--common] [--verbose]");
        std::process::exit(1);
    }

    // warmup
    if let Ok(img) = image::open(&files[0]) {
        test_zxing_cpp(&img.into_luma8(), limit_formats);
    }

    bench(|image| test_zxing_cpp(image, limit_formats), "zxing-cpp", &files, verbose);
    bench(|image| test_rxing(image, limit_formats), "rxing", &files, verbose);
    bench(|image| test_zbar_rust(image), "zbar-rust", &files, verbose);
    bench(|image| test_zedbar(image, limit_formats), "zedbar", &files, verbose);
    bench(|image| test_bardecoder(image), "bardecoder", &files, verbose);
    bench(|image| test_rqrr(image), "rqrr", &files, verbose);
    bench(|image| test_zxing_cpp(image, limit_formats), "zxing-cpp", &files, verbose);
}
