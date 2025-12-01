using iText.Kernel.Pdf;

namespace Infrastructure.Services.CandDocs
{
    public static class PdfUtils
    {
        public static async Task<List<byte[]>> SplitPdfByPageAsync(byte[] pdfBytes)
        {
            var list = new List<byte[]>();
            using var ms = new MemoryStream(pdfBytes);
            using var reader = new PdfReader(ms);
            using var src = new PdfDocument(reader);
            int n = src.GetNumberOfPages();

            for (int i = 1; i <= n; i++)
            {
                using var outMs = new MemoryStream();
                using var writer = new PdfWriter(outMs);
                using var dest = new PdfDocument(writer);
                src.CopyPagesTo(i, i, dest);
                dest.Close();
                list.Add(outMs.ToArray());
            }
            return list;
        }

        public static async Task<byte[]> MergeTwoPagesAsync(byte[] p1, byte[] p2)
        {
            using var outMs = new MemoryStream();
            using var writer = new PdfWriter(outMs);
            using var dest = new PdfDocument(writer);
            using (var r1 = new PdfReader(new MemoryStream(p1)))
            using (var src1 = new PdfDocument(r1))
            {
                src1.CopyPagesTo(1, src1.GetNumberOfPages(), dest);
            }
            if (p2 != null)
            {
                using (var r2 = new PdfReader(new MemoryStream(p2)))
                using (var src2 = new PdfDocument(r2))
                {
                    src2.CopyPagesTo(1, src2.GetNumberOfPages(), dest);
                }
            }
            dest.Close();
            return outMs.ToArray();
        }
    }
}
