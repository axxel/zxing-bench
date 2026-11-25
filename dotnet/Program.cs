using System.Collections.Generic;
using SkiaSharp;
using ZXingCpp;
using ZXing;
using ZXing.SkiaSharp;
using FeatherQR;
using FeatherQR.SkiaSharp;
using Dynamsoft.DBR;
using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.License;
using System.Runtime.InteropServices;
using System.Globalization;
using ZXingNetBF = global::ZXing.BarcodeFormat;
using ZXingCppBF = global::ZXingCpp.BarcodeFormat;
// using IronBarCode;

public static class SkBitmapBarcodeReader
{
    public static Barcode[] Read(SKBitmap img, ReaderOptions? opts = null)
    {
        var format = img.Info.ColorType switch
        {
            SKColorType.Gray8 => ImageFormat.Lum,
            SKColorType.Rgba8888 => ImageFormat.RGBA,
            SKColorType.Bgra8888 => ImageFormat.BGRA,
            _ => ImageFormat.None,
        };
        if (format == ImageFormat.None)
        {
            if (!img.CanCopyTo(SKColorType.Gray8))
                throw new Exception("Incompatible SKColorType");
            img = img.Copy(SKColorType.Gray8);
            format = ImageFormat.Lum;
        }
        return ZXingCpp.BarcodeReader.Read(img.GetPixels(), img.RowBytes * img.Info.Height, img.Info.Width, img.Info.Height,
            format, 0, 0, opts);
    }

    public static Barcode[] Read(this ZXingCpp.BarcodeReader reader, SKBitmap img) => Read(img, reader);
}

public class Program
{
    public static bool SingleBarcodeMode = false;
    public static bool QRCodeOnly = false;

    public static Func<string, int> PrimeFileCache()
    {
        return (filename) =>
        {
            using (var img = SKBitmap.Decode(filename))
            {
                return 0;
            }
        };
    }

    // public static Func<string, int> IronBarCodeFn()
    // {
    //     IronBarCode.License.LicenseKey = "IRONSUITE.ZXINGCPP.GMAIL.COM.9293-D39AE865B5-BCU3NTXRHF7GXW6K-WIXQXWKCZG6P-LJT5MB52HZUX-4MMUTT3CNR7K-LR6R6GIYCJVB-OWXUNQI7RR24-SMRUH3-TZUVSMLHB26MEA-DEPLOYMENT.TRIAL-7EOSLJ.TRIAL.EXPIRES.08.MAY.2024";
    //     var myOptions = new BarcodeReaderOptions()
    //     {
    //         ExpectBarcodeTypes = BarcodeEncoding.AllOneDimensional
    //     };

    //     return (filename) =>
    //     {
    //         return IronBarCode.BarcodeReader.Read(filename).Count;
    //     };
    // }

    public static Func<string, int> ZXing()
    {
        var reader = new ZXing.SkiaSharp.BarcodeReader
        {
            AutoRotate = true,
            Options =
            {
                TryHarder = true,
                TryInverted = true,
            }
        };
        if (QRCodeOnly)
            reader.Options.PossibleFormats = new List<ZXingNetBF> { ZXingNetBF.QR_CODE };

        return (filename) =>
        {
            using (var img = SKBitmap.Decode(filename))
            {
                var source = new SKBitmapLuminanceSource(img);
                if (SingleBarcodeMode)
                    return reader.Decode(source) == null ? 0 : 1;
                else
                    return reader.DecodeMultiple(source)?.Length ?? 0;
            }
        };
    }

    public static Func<string, int> ZXingCpp()
    {
        var reader = new ZXingCpp.BarcodeReader
        {
            Formats = QRCodeOnly ? ZXingCppBF.QRCode : ZXingCppBF.All
        };
        if (SingleBarcodeMode)
            reader.MaxNumberOfSymbols = 1;

        return (filename) =>
        {
            using (var img = SKBitmap.Decode(filename))
            {
                return reader.Read(img).Length;
            }
        };
    }

    public static Func<string, int> FeatherQR()
    {
        return (filename) =>
        {
            using (var img = SKBitmap.Decode(filename))
            {
                if (SingleBarcodeMode)
                    return QRCodeDecoder.TryDecode(img, out var _) ? 1 : 0;
                else
                    return SymbolDecoder.DecodeSymbols(img).Length;
            }
        };
    }

    public static Func<string, int> Dynamsofti()
    {
        string errorMsg;
        int errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
        if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_LICENSE_WARNING)
            Console.WriteLine(errorMsg);

        CaptureVisionRouter cvRouter = new CaptureVisionRouter();
        if (cvRouter.GetSimplifiedSettings(PresetTemplate.PT_READ_BARCODES, out var settings) == 0 && settings != null)
        {
            settings.barcodeSettings.barcodeFormatIds = QRCodeOnly
                ? (ulong)EnumBarcodeFormat.BF_QR_CODE | (ulong)EnumBarcodeFormat.BF_MICRO_QR
                : (ulong)EnumBarcodeFormat.BF_ALL;
            if (SingleBarcodeMode)
                settings.barcodeSettings.expectedBarcodesCount = 1;
            cvRouter.UpdateSettings(PresetTemplate.PT_READ_BARCODES, settings);
        }

        return (filename) =>
        {
            return cvRouter.CaptureMultiPages(filename, PresetTemplate.PT_READ_BARCODES)?.Length ?? 0;
        };
    }

    public static void Bench(string name, ICollection<string> files, Func<string, int> f)
    {
        name = name.PadRight(16);
        Console.Write($"{name} ");
        var watch = System.Diagnostics.Stopwatch.StartNew();
        int n = 0;
        int i = 0;
        foreach (var fn in files)
        {
            i++;
            Console.Write($"\r\u001b[K{name} {i} of {files.Count}");
            try
            {
                n += f(fn);
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n  FAILED: {fn}: {e}");
            }
        }
        watch.Stop();
        var timeStr = files.Count == 1
            ? $"{watch.Elapsed.TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture),5}ms"
            : $"{watch.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture),5}s";
        Console.WriteLine($"\r\u001b[K{name} found {n,4} barcodes in {files.Count} images in {timeStr}");
    }

    public static void Main(string[] args)
    {
        string[] endings = {".png", ".jpg", ".jpeg", ".webp"};
        List<string> files = new();
        foreach (var arg in args)
        {
            if (Directory.Exists(arg))
                foreach (var file in Directory.EnumerateFiles(arg, "*.*", SearchOption.AllDirectories).Where(x => endings.Any(x.EndsWith)))
                    files.Add(file);
            else if (arg == "--single")
                SingleBarcodeMode = true;
            else if (arg == "--qrcode")
                QRCodeOnly = true;
            else
                files.Add(arg);
        }

        Bench("PrimeFileCache", files, PrimeFileCache());
        Bench("ZXingCpp", files, ZXingCpp());

        // Bench("IronBarCode", files, IronBarCodeFn());
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            Bench("Dynamsoft", files, Dynamsofti());
        Bench("ZXing.Net", files, ZXing());
        Bench("FeatherQR", files, FeatherQR());
        Bench("ZXingCpp", files, ZXingCpp());
    }
}
