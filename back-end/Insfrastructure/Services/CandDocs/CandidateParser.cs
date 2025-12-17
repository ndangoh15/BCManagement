using Domain.InterfacesServices.CandDocs;
using Domain.Models.CandDocs;
using Infrastructure.Utils;
using System.Text.RegularExpressions;

namespace Infrastructure.Services.CandDocs
{
    public class CandidateParser : ICandidateParser
    {
        // ================= CLEAN OCR =================
        private string CleanOcrText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text ?? "";

            string cleaned = TextUtils.RemoveAccents(text);

            cleaned = Regex.Replace(cleaned, @"[^\x20-\x7E\r\n\tÀ-ÿ]", " ");

            cleaned = cleaned.Replace("|", " ")
                             .Replace("¦", " ")
                             .Replace("—", " ")
                             .Replace("~", " ")
                             .Replace("=", " ")
                             .Replace(";", ":");

            cleaned = Regex.Replace(cleaned, @"(?m)^\s*[\d\s\|\-]{1,}\s+(?=[A-Za-zÀ-ÿ])", "");

            cleaned = Regex.Replace(cleaned, @"\t+", " ");
            cleaned = Regex.Replace(cleaned, @"[ ]{2,}", " ");

            cleaned = Regex.Replace(cleaned, @"C[\.\s]*I[\.\s]*N", "CIN", RegexOptions.IgnoreCase);
            cleaned = cleaned.Replace("C.LLN", "CIN")
                             .Replace("C.LN", "CIN")
                             .Replace("C.1.N", "CIN")
                             .Replace("C.L.N", "CIN")
                             .Replace("C.ILN", "CIN");

            cleaned = Regex.Replace(
                cleaned,
                @"Examination\s*Cent(er|re)\s*[:\-]?",
                "EXAMINATION_CENTRE:",
                RegexOptions.IgnoreCase
            );

            cleaned = cleaned.Replace("Bxamination Centre", "EXAMINATION_CENTRE")
                             .Replace("Exatnitiation Centre", "EXAMINATION_CENTRE")
                             .Replace("Examination Contre.", "EXAMINATION_CENTRE:")
                             .Replace("Examination Centre.", "EXAMINATION_CENTRE:");

            return cleaned.Trim();
        }

        // ================= HELPERS =================
        private string ToUpperClean(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.ToUpperInvariant();
            return Regex.Replace(s, @"\s{2,}", " ").Trim();
        }

        // ================= CIN + NAME (STRICT) =================
        private (string cin, string name) ExtractCinAndNameStrict(string[] lines)
        {
            foreach (var line in lines)
            {
                if (!Regex.IsMatch(line, @"\bCIN\b", RegexOptions.IgnoreCase))
                    continue;

                // ignore Payment / Receipt
                if (Regex.IsMatch(line, @"PAYMENT|RECEIPT", RegexOptions.IgnoreCase))
                    continue;

                var m = Regex.Match(line, @"\b(\d{8,10})\b");
                if (!m.Success)
                    continue;

                var cin = m.Groups[1].Value;

                var namePart = line[(m.Index + m.Length)..].Trim();
                namePart = Regex.Replace(namePart, @"^[^A-Z]+", "");
                namePart = Regex.Replace(namePart, @"[^\p{L}\s\-']", "");
                namePart = Regex.Replace(namePart, @"\s{2,}", " ").Trim();

                if (Regex.IsMatch(namePart, @"GENERAL|CERTIFICATE|BOARD|TIMETABLE", RegexOptions.IgnoreCase))
                    return ("", "");

                return (cin, ToUpperClean(namePart));
            }

            return ("", "");
        }

        // ================= CENTRE (SAFE) =================
        private string ExtractCentreNumberSafe(string[] lines)
        {
            const int MAX_LOOKAHEAD = 10;

            for (int i = 0; i < lines.Length; i++)
            {
                //if (!Regex.IsMatch(
                //        lines[i],
                //        @"EXAMINATION\s*CENT(RE|ER)|CENTRE\s+D[' ]?EXAMEN",
                //        RegexOptions.IgnoreCase))
                //    continue;
                if (!lines[i].ToUpperInvariant().Contains("EXAMINATION") || !lines[i].ToUpperInvariant().Contains("CENT"))
                {
                    continue;
                }


                // 1️⃣ même ligne
                var sameLine = Regex.Match(lines[i], @"\b(\d{5})\b");
                if (sameLine.Success && !IsYear(sameLine.Value))
                    return sameLine.Value;

                // 2️⃣ lignes suivantes
                int inspected = 0;
                for (int j = i + 1; j < lines.Length && inspected < MAX_LOOKAHEAD; j++)
                {
                    inspected++;
                    var l = lines[j].Trim();
                    if (string.IsNullOrWhiteSpace(l)) continue;

                    var m = Regex.Match(l, @"\b(\d{5})\b");
                    if (m.Success && !IsYear(m.Value))
                        return m.Value;
                }

                break;
            }

            return "";
        }

        private bool IsYear(string value)
        {
            if (!int.TryParse(value, out var y)) return false;
            return y >= 1990 && y <= DateTime.Now.Year + 1;
        }



        // ================= SESSION =================
        private int? ExtractSessionYear(string text)
        {
            var m = Regex.Match(text, @"\b(June|Juin)\s+(\d{4})\b", RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[2].Value, out var y))
                return y;

            m = Regex.Match(text, @"\b(20\d{2})\b");
            if (m.Success && int.TryParse(m.Groups[1].Value, out y))
                return y;

            return null;
        }

        // ================= MAIN PARSE =================
        public CandidateInfo Parse(string ocrText)
        {
            var info = new CandidateInfo
            {
                RawOcrText = ocrText ?? ""
            };

            if (string.IsNullOrWhiteSpace(ocrText))
                return info;

            var cleaned = CleanOcrText(ocrText);

            var lines = cleaned
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            // CIN + NAME
            var (cin, name) = ExtractCinAndNameStrict(lines);
            info.CandidateNumber = cin;
            info.CandidateName = name;

            // CENTRE
            info.CentreNumber = ExtractCentreNumberSafe(lines);

            // SESSION
            info.SessionYear = ExtractSessionYear(cleaned);

            return info;
        }
    }
}
