using System.Collections.Generic;
using ImageMagick;
using ZXingCpp;
using ZXing;
using ZXing.Magick;
using Dynamsoft;
using Dynamsoft.DBR;

public static class ImageMagickBarcodeReader
{
    public static List<Barcode> Read(MagickImage img, ReaderOptions? opts = null)
    {
        if (img.DetermineBitDepth() < 8)
            img.SetBitDepth(8);
        var bytes = img.ToByteArray(MagickFormat.Gray);
        var iv = new ImageView(bytes, img.Width, img.Height, ImageFormat.Lum, 0, 0);
        return ZXingCpp.BarcodeReader.Read(iv, opts);
    }

    public static List<Barcode> Read(this ZXingCpp.BarcodeReader reader, MagickImage img) => Read(img, reader);
}

public class Program
{
    public static Func<string, int> ZXing()
    {
        var reader = new ZXing.Magick.BarcodeReader { AutoRotate = true };
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
            var img = new MagickImage(filename);
            var source = new MagickImageLuminanceSource(img);
            Result[] barcodes = reader.DecodeMultiple(source);
            if (barcodes is null)
                return 0;
            foreach (var b in barcodes)
                Console.WriteLine($"  {b.BarcodeFormat} : {b.Text}");
            return barcodes.Length;
        };
    }

    public static Func<string, int> ZXingCpp()
    {
        var reader = new ZXingCpp.BarcodeReader();

        return (filename) =>
        {
            var img = new MagickImage(filename);
            var barcodes = reader.Read(img);
            foreach (var b in barcodes)
                Console.WriteLine($"  {b.Format} : {b.Text}");
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

            foreach (var b in results)
                Console.WriteLine($"  {b.BarcodeFormatString} : {b.BarcodeText}");
            return results.Length;
        };
    }

    public static void Bench(string name, string[] files, Func<string, int> f)
    {
        Console.WriteLine($"Starting scan with {name}...");
        var watch = System.Diagnostics.Stopwatch.StartNew();
        int n = 0;
        foreach (var fn in files)
            n += f(fn);
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Console.WriteLine($"{name} found {n} barcodes in {elapsedMs}ms\n");
    }

    public static void Main(string[] args)
    {
        Bench("ZXing.Net", args, ZXing());
        Bench("Dynamsoft", args, Dynamsofti());
        Bench("ZXingCpp ", args, ZXingCpp());
    }
}