using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.ImportErrors
{
    public class CandidateDocumentEditDto
    {
        public int Id { get; set; }

        public int Session { get; set; }
        public string ExamCode { get; set; }
        public string CentreCode { get; set; }

        public string CandidateNumber { get; set; }
        public string CandidateName { get; set; }

        public string FilePath { get; set; }
        public string PdfUrl { get; set; }

        public List<ImportErrorDto> Errors { get; set; }
    }

}
