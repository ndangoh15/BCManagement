using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.InterfacesStores.CandDocs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.CandDocs.Queries
{
    public class GetCandidateDocumentFileHandler   : IRequestHandler<GetCandidateDocumentFileQuery, FileStreamResult?>
    {
        private readonly ICandidateRepository _repository;

        public GetCandidateDocumentFileHandler(ICandidateRepository repository)
        {
            _repository = repository;
        }

        public async Task<FileStreamResult?> Handle(GetCandidateDocumentFileQuery request,CancellationToken cancellationToken)
        {
            var doc = await _repository.GetByIdAsync(request.DocumentId);
            if (doc == null || !System.IO.File.Exists(doc.FilePath))
                return null;

            var stream = System.IO.File.OpenRead(doc.FilePath);
            return new FileStreamResult(stream, "application/pdf");
        }
    }
}
