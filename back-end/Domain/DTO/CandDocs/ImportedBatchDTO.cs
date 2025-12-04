using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.CandDocs
{
    public class ImportedBatchDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public int ExamYear { get; set; }
        public string ExamCode { get; set; }
        public string CentreNumber { get; set; }
        public DateTime ImportedAt { get; set; }
    }
}
