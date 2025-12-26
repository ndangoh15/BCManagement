using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Requests
{
    public class SearchCandidateDocumentsRequest
    {
        // 🔴 SEUL champ obligatoire
        public string CandidateName { get; set; } = string.Empty;

        // 🟢 Champs optionnels
        public string? CandidateNumber { get; set; }
        public string? CenterNumber { get; set; }
        public string? ExamCode { get; set; }
        public int? Session { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }


}
