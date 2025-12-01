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
        private readonly TesseractEngine _engine;

        public TesseractService(string tessdataPath, string language = "eng")
        {
            _tessdataPath = tessdataPath;

            _engine = new TesseractEngine(_tessdataPath, language, EngineMode.LstmOnly);

            // OCR tuning for GCE Cameroon birth certificates
            _engine.SetVariable("tessedit_char_whitelist",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-/(). ");
            _engine.SetVariable("load_system_dawg", "0");
            _engine.SetVariable("load_freq_dawg", "0");
        }

        // ===================================================
        // PUBLIC: Extract text directly from an image
        // ===================================================
        public string ExtractTextFromImage(byte[] imageBytes)
        {
            return ExtractWithTesseract(imageBytes);
        }

        public async Task<string> ExtractTextFromImageAsync(byte[] imageBytes)
        {
            return await Task.Run(() => ExtractWithTesseract(imageBytes));
        }

        // ===================================================
        // PUBLIC: Extract text from PDF (page render via Magick)
        // ===================================================
        public async Task<string> ExtractTextFromPdfAsync(byte[] pdfBytes, int pageNumber = 1)
        {
            if (pdfBytes == null)
                throw new ArgumentNullException(nameof(pdfBytes));

            try
            {
                var settings = new MagickReadSettings
                {
                    Density = new Density(300),
                    FrameIndex = (uint)(pageNumber - 1),
                    FrameCount = 1
                };

                using var images = new MagickImageCollection();
                using var ms = new MemoryStream(pdfBytes);
                images.Read(ms, settings);

                using var img = (MagickImage)images[0];

                // Improve image for OCR
                PreprocessImage(img);

                using var pngStream = new MemoryStream();
                await img.WriteAsync(pngStream, MagickFormat.Png);

                return ExtractWithTesseract(pngStream.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception($"OCR PDF error: {ex.Message}", ex);
            }
        }

        // ===================================================
        // IMAGE PREPROCESSING PIPELINE (BEST SETTINGS)
        // ===================================================
        private void PreprocessImage(MagickImage img)
        {
            img.ColorType = ColorType.Grayscale;
            img.Normalize();
            img.Enhance();

            // Smart sharpening
            img.Sharpen();

            // Deskew
            img.Deskew(new Percentage(60));

            // Remove noise
            img.ReduceNoise();
        }

        // ===================================================
        // TESSERACT CALL
        // ===================================================
        private string ExtractWithTesseract(byte[] imageBytes)
        {
            using var img = Pix.LoadFromMemory(imageBytes);
            using var page = _engine.Process(img);
            return page.GetText();
        }

        public void Dispose()
        {
            _engine?.Dispose();
        }
    }
}
