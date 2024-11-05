using System.Collections.Generic;
using SkiaSharp;
using ZXingCpp;
using ZXing;
using ZXing.SkiaSharp;
using Dynamsoft;
using Dynamsoft.DBR;
using QRCodeDecoderLibrary;
using IronBarCode;

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
		var iv = new ImageView(img.GetPixels(), img.Info.Width, img.Info.Height, format);
		return ZXingCpp.BarcodeReader.Read(iv, opts);
	}

	public static Barcode[] From(this ZXingCpp.BarcodeReader reader, SKBitmap img) => Read(img, reader);
}

public class Program
{
    public static bool SingleBarcodeMode = false;

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


    public static Func<string, int> QRCodeDecoderLibrary()
    {
        var read = new QRDecoder();
        // if (SingleBarcodeMode)
        //     read.MaxNumberOfSymbols = 1;

        return (filename) =>
        {
            using (var img = new System.Drawing.Bitmap(filename))
            {
                return read.ImageDecoder(img).Length;
            }
        };
    }

    public static Func<string, int> IronBarCodeFn()
    {
        IronBarCode.License.LicenseKey = "IRONSUITE.ZXINGCPP.GMAIL.COM.9293-D39AE865B5-BCU3NTXRHF7GXW6K-WIXQXWKCZG6P-LJT5MB52HZUX-4MMUTT3CNR7K-LR6R6GIYCJVB-OWXUNQI7RR24-SMRUH3-TZUVSMLHB26MEA-DEPLOYMENT.TRIAL-7EOSLJ.TRIAL.EXPIRES.08.MAY.2024";
        var myOptions = new BarcodeReaderOptions()
        {
            ExpectBarcodeTypes = BarcodeEncoding.AllOneDimensional
        };

        return (filename) =>
        {
            return IronBarCode.BarcodeReader.Read(filename).Count;
        };
    }

    public static Func<string, int> ZXing()
    {
        var reader = new ZXing.SkiaSharp.BarcodeReader
        {
            AutoRotate = true,
            Options =
            {
                TryHarder = true,
                TryInverted = true,
                PossibleFormats = new List<BarcodeFormat>()
                {
                    BarcodeFormat.UPC_A,
                    BarcodeFormat.UPC_E,
                    BarcodeFormat.EAN_13,
                    BarcodeFormat.EAN_8,
                    BarcodeFormat.RSS_14,
                    BarcodeFormat.RSS_EXPANDED,
                    BarcodeFormat.CODE_39,
                    BarcodeFormat.CODE_93,
                    BarcodeFormat.CODE_128,
                    BarcodeFormat.ITF,
                    BarcodeFormat.QR_CODE,
                    BarcodeFormat.DATA_MATRIX,
                    BarcodeFormat.AZTEC,
                    BarcodeFormat.PDF_417,
                    BarcodeFormat.CODABAR,
                    BarcodeFormat.MAXICODE,
                },
            }
        };

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
        var read = new ZXingCpp.BarcodeReader();
        if (SingleBarcodeMode)
            read.MaxNumberOfSymbols = 1;

        return (filename) =>
        {
            using (var img = SKBitmap.Decode(filename))
            {
                return read.From(img).Length;
            }
        };
    }

    public static Func<string, int> Dynamsofti()
    {
        string errorMsg;
        EnumErrorCode errorCode = Dynamsoft.DBR.BarcodeReader.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
        if (errorCode != EnumErrorCode.DBR_SUCCESS)
            Console.WriteLine(errorMsg);

        var dbr = Dynamsoft.DBR.BarcodeReader.GetInstance();
        var settings = dbr.GetRuntimeSettings();
        settings.BarcodeFormatIds = (int)EnumBarcodeFormat.BF_ALL
            ^ (int)(EnumBarcodeFormat.BF_MSI_CODE | EnumBarcodeFormat.BF_PATCHCODE | EnumBarcodeFormat.BF_MAXICODE
                    | EnumBarcodeFormat.BF_INDUSTRIAL_25 | EnumBarcodeFormat.BF_GS1_DATABAR_LIMITED);

        // settings.BinarizationModes[0] = EnumBinarizationMode.BM_LOCAL_BLOCK;
        // settings.DeblurModes[0] = EnumDeblurMode.DM_BASED_ON_LOC_BIN;
        // settings.DeblurModes[1] = EnumDeblurMode.DM_THRESHOLD_BINARIZATION;
        // settings.MaxAlgorithmThreadCount = 1;
        if (SingleBarcodeMode)
            settings.ExpectedBarcodesCount = 1;

        dbr.UpdateRuntimeSettings(settings);

        return (filename) =>
        {
            return dbr.DecodeFile(filename, "")?.Length ?? 0;
        };
    }

    public static void Bench(string name, ICollection<string> files, Func<string, int> f)
    {
        Console.Write($"Starting scan with {name} ");
        var watch = System.Diagnostics.Stopwatch.StartNew();
        int n = 0;
        foreach (var fn in files)
        {
            Console.Write(".");
            try
            {
                // Console.WriteLine($"{fn}");
                n += f(fn);
            }
            catch (Exception e)
            {
                Console.WriteLine($"  FAILED: {fn}: {e}");
            }
        }
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Console.WriteLine($"\n{name} found {n,3} barcodes in {files.Count} images in {elapsedMs,4}ms\n");
    }

    public static void Main(string[] args)
    {
        string[] endings = {".png", ".jpg", ".jpeg"};
        List<string> files = new();
        foreach (var arg in args)
        {
            if (Directory.Exists(arg))
                foreach (var file in Directory.EnumerateFiles(arg, "*.*", SearchOption.AllDirectories).Where(x => endings.Any(x.EndsWith)))
                    files.Add(file);
            else if (arg == "--single")
                SingleBarcodeMode = true;
            else
                files.Add(arg);
        }

        Bench("PrimeFileCache...", files, PrimeFileCache());
        Bench("ZXingCpp   ", files, ZXingCpp());

        // Bench("QRC.Dec..", files, QRCodeDecoderLibrary());
        // Bench("IronBarCode", files, IronBarCodeFn());
        Bench("Dynamsoft  ", files, Dynamsofti());
        Bench("ZXing.Net  ", files, ZXing());
        Bench("ZXingCpp   ", files, ZXingCpp());
    }
}
