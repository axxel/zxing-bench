using System.Collections.Generic;
using SkiaSharp;
using ZXingCpp;
using ZXing;
using ZXing.SkiaSharp;
using Dynamsoft;
using Dynamsoft.DBR;

// public static class ImageMagickBarcodeReader
// {
//     public static List<Barcode> Read(MagickImage img, ReaderOptions? opts = null)
//     {
//         if (img.DetermineBitDepth() < 8)
//             img.SetBitDepth(8);
//         var bytes = img.ToByteArray(MagickFormat.Gray);
//         var iv = new ImageView(bytes, img.Width, img.Height, ImageFormat.Lum, 0, 0);
//         return ZXingCpp.BarcodeReader.Read(iv, opts);
//     }

//     public static List<Barcode> Read(this ZXingCpp.BarcodeReader reader, MagickImage img) => Read(img, reader);
// }

public static class SkBitmapBarcodeReader
{
	public static List<Barcode> Read(SKBitmap img, ReaderOptions? opts = null)
	{
		var format = img.Info.ColorType switch
		{
			SKColorType.Gray8 => ImageFormat.Lum,
			SKColorType.Rgba8888 => ImageFormat.RGBX,
			SKColorType.Bgra8888 => ImageFormat.BGRX,
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

	public static List<Barcode> Read(this ZXingCpp.BarcodeReader reader, SKBitmap img) => Read(img, reader);
}

public class Program
{
    public static Func<string, int> ZXing()
    {
        var reader = new ZXing.SkiaSharp.BarcodeReader { AutoRotate = true };
        reader.Options.Hints.Add(DecodeHintType.TRY_HARDER, true);
        reader.Options.Hints.Add(DecodeHintType.ALSO_INVERTED, true);
        var vector = new List<BarcodeFormat>()
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
        };
        reader.Options.Hints.Add(DecodeHintType.POSSIBLE_FORMATS, vector);

        return (filename) =>
        {
            var img = SKBitmap.Decode(filename);
            var source = new SKBitmapLuminanceSource(img);
            Result[] barcodes = reader.DecodeMultiple(source);
            if (barcodes is null)
                return 0;
            // foreach (var b in barcodes)
            //     Console.WriteLine($"  {b.BarcodeFormat} : {b.Text}");
            return barcodes.Length;
        };
    }

    public static Func<string, int> ZXingCpp()
    {
        var reader = new ZXingCpp.BarcodeReader();

        return (filename) =>
        {
            var img = SKBitmap.Decode(filename);
            var barcodes = reader.Read(img);
            // foreach (var b in barcodes)
            //     Console.WriteLine($"  {b.Format} : {b.Text}");
            return barcodes.Count;
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

        dbr.UpdateRuntimeSettings(settings);

        return (filename) =>
        {
            var results = dbr.DecodeFile(filename, "");

            if (results == null)
                return 0;

            // foreach (var b in results)
            //     Console.WriteLine($"  {b.BarcodeFormatString} : {b.BarcodeText}");
            return results.Length;
        };
    }

    public static void Bench(string name, IEnumerable<string> files, Func<string, int> f)
    {
        Console.Write($"Starting scan with {name} ");
        var watch = System.Diagnostics.Stopwatch.StartNew();
        int n = 0;
        foreach (var fn in files)
        {
            Console.Write(".");
            try
            {
                n += f(fn);
            }
            catch (Exception e)
            {
                Console.WriteLine($"  FAILED: {fn}: {e}");
            }
        }
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Console.WriteLine($"\n{name} found {n} barcodes in {elapsedMs}ms\n");
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
            else
                files.Add(arg);
        }

        Bench("ZXing.Net", files, ZXing());
        Bench("Dynamsoft", files, Dynamsofti());
        Bench("ZXingCpp ", files, ZXingCpp());
    }
}
