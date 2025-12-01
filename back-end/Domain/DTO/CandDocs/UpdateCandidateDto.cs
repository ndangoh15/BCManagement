using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.CandDocs
{
    public class UpdateCandidateDto
    {
        public int DocumentId { get; set; }
        public string CandidateNumber { get; set; }
        public string CandidateName { get; set; }
        public string CentreCode { get; set; }
    }
}
