using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.CandDocs
{
    public class CandidateDocumentDto
    {
        public int Id { get; set; }

        public string CandidateNumber { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;

        public string CentreCode { get; set; } = string.Empty;
        public string ExamCode { get; set; } = string.Empty;
        public int Session { get; set; }

        // 🔗 Pour ouvrir / preview le PDF
        public string FilePath { get; set; } = string.Empty;
    }

}
