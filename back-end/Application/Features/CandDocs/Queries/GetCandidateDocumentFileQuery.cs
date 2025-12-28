using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.CandDocs.Queries
{
    public class GetCandidateDocumentFileQuery : IRequest<FileStreamResult?>
    {
        public int DocumentId { get; set; }
    }
}
