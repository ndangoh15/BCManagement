using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Domain.DTO.CandDocs
{
    public class UploadBatchRequestDTO
    {
        public string ServerSourceFilePath { get; set; }

        public int ExamYear { get; set; }
        public string ExamCode { get; set; }
        public string CenterNumber { get; set; }

        public int UploadedBy { get; set; }
    }
}
