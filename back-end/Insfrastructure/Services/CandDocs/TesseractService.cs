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

        public async Task<string> ExtractTextFromPdfAsync(byte[] pdfBytes, int pageNumber = 1)
        {
            if (pdfBytes == null || pdfBytes.Length == 0)
                return string.Empty;

            // TEMP isolé par OCR
            var tempDir = Path.Combine(
                Path.GetTempPath(),
                "ocr",
                Guid.NewGuid().ToString()
            );

            Directory.CreateDirectory(tempDir);
            MagickNET.SetTempDirectory(tempDir);

            try
            {
                var settings = new MagickReadSettings
                {
                    Density = new Density(200),
                    FrameIndex = (uint)(pageNumber - 1),
                    FrameCount = 1
                    //  PAS de Format ici
                };

                using var ms = new MemoryStream(pdfBytes);
                using var images = new MagickImageCollection();
                images.Read(ms, settings); // lit le PDF via Ghostscript

                if (images.Count == 0)
                    return string.Empty;

                using var img = (MagickImage)images[0];

                img.ColorType = ColorType.Grayscale;
                img.Normalize();
                img.Despeckle();

                using var pngStream = new MemoryStream();
                await img.WriteAsync(pngStream, MagickFormat.Png); //  PNG ICI

                return ExtractWithTesseract(pngStream.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("OCR error: " + ex.Message, ex);
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }



        /*public async Task<string> ExtractTextFromPdfAsync(byte[] pdfBytes, int pageNumber = 1)
        {
            try
            {
                if (pdfBytes == null || pdfBytes.Length == 0)
                    return string.Empty;

                var settings = new MagickReadSettings
                {
                    Density = new Density(150),     // 200 is enough
                    FrameIndex = (uint)(pageNumber - 1),
                    FrameCount = 1,
                    Format = MagickFormat.Png
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
        }*/

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
