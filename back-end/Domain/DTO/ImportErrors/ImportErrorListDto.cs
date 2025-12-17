using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.ImportErrors
{
    public class ImportErrorListDto
    {
        public int ErrorId { get; set; }
        public int DocumentId { get; set; }

        public int Session { get; set; }
        public string ExamCode { get; set; }
        public string CentreCode { get; set; }

        public string CandidateNumber { get; set; }
        public string CandidateName { get; set; }

        public string FieldName { get; set; }
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }

        public string FilePath { get; set; }
        public DateTime ImportDate { get; set; }
    }

}
