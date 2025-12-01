using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Tesseract;
using ImageMagick;
using Domain.InterfacesServices.CandDocs;
using SkiaSharp;

namespace Infrastructure.Services.CandDocs
{
    public class TesseractService : ITesseractService, IDisposable
    {

        private readonly string _tessdataPath;
        private readonly EngineMode _engineMode = EngineMode.Default;
        public TesseractService(string tessdataPath)
        {
            _tessdataPath = tessdataPath ?? throw new ArgumentNullException(nameof(tessdataPath));
        }

        //private readonly TesseractEngine _engine;

        //public TesseractService(string tessdataPath, string language = "eng")
        //{
        //    if (!Directory.Exists(tessdataPath)) throw new DirectoryNotFoundException(tessdataPath);
        //    _engine = new TesseractEngine(tessdataPath, language, EngineMode.LstmOnly);
        //    _engine.SetVariable("load_system_dawg", "0");
        //    _engine.SetVariable("load_freq_dawg", "0");
        //    _engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-/. ");
        //}

        public async Task<string> ExtractTextFromPdfAsync(byte[] pdfBytes, int pageNumber = 1)
        {
            try
            {
                if (pdfBytes == null || pdfBytes.Length == 0)
                    return string.Empty;

                var settings = new MagickReadSettings
                {
                    Density = new Density(200),     // 200 is enough
                    FrameIndex = (uint)(pageNumber - 1),
                    FrameCount = 1
                };

                using var ms = new MemoryStream(pdfBytes);
                using var images = new MagickImageCollection();
                images.Read(ms, settings);

                if (images.Count == 0)
                    return string.Empty;

                using var img = (MagickImage)images[0];

                // BEST SETTINGS FOR GCE DOCUMENTS
                img.ColorType = ColorType.Grayscale;
                img.Normalize();          // Keep only Normalize()
                img.Despeckle();          // Light – this one is safe

                // ❌ DO NOT SHARPEN
                // ❌ DO NOT ENHANCE
                // ❌ DO NOT DESKEW (GCE is already aligned)
                // ❌ DO NOT REDUCE NOISE

                using var pngStream = new MemoryStream();
                await img.WriteAsync(pngStream, MagickFormat.Png);

                return ExtractWithTesseract(pngStream.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("OCR error: " + ex.Message, ex);
            }
        }

        private string ExtractWithTesseract(byte[] imageBytes)
        {
            // Create engine using tessdata path. Languages: adjust ("eng+fra") as needed.
            using var engine = new TesseractEngine(_tessdataPath, "eng+fra", _engineMode);
            using var img = Pix.LoadFromMemory(imageBytes);
            using var page = engine.Process(img);
            var text = page.GetText();
            return text ?? string.Empty;
        }

        public Task<string> ExtractTextFromImageAsync(byte[] imageBytes)
        {
            if (imageBytes == null) throw new ArgumentNullException(nameof(imageBytes));
            var text = ExtractWithTesseract(imageBytes);
            return Task.FromResult(text);
        }

        public void Dispose()
        {
            // no global resources to dispose
        }
    }
}
