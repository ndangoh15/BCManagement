using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.CandDocs
{
    public class UploadFormDto
    {
        public IFormFile File { get; set; }

        public int ExamYear { get; set; }
        public string ExamCode { get; set; } = "";
        public string CenterNumber { get; set; } = "";

        //public int UploadedBy { get; set; }   // you can replace with string username if needed
    }
}
