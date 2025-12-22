using Domain.DTO.ImportErrors;
using Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CandDocs.Queries
{
    public class GetImportErrorDetailHandler
    : IRequestHandler<GetImportErrorDetailQuery, CandidateDocumentEditDto>
    {
        private readonly FsContext _context;

        public GetImportErrorDetailHandler(FsContext context)
        {
            _context = context;
        }

        public async Task<CandidateDocumentEditDto> Handle(GetImportErrorDetailQuery request, CancellationToken cancellationToken)
        {
            var error = await _context.ImportErrors
                .Include(e => e.CandidateDocument)
                .Where(e => e.Id == request.ErrorId)
                .Select(e => new CandidateDocumentEditDto
                {
                    Id = e.CandidateDocument.Id,
                    Session = e.CandidateDocument.Session,
                    ExamCode = e.CandidateDocument.ExamCode,
                    CentreCode = e.CandidateDocument.CentreCode,
                    CandidateNumber = e.CandidateDocument.CandidateNumber,
                    CandidateName = e.CandidateDocument.CandidateName,
                    FilePath = e.CandidateDocument.FilePath,
                    PdfUrl = $"/api/files/preview/{e.CandidateDocument.Id}",
                    Errors = new List<ImportErrorDto>
                    {
                    new ImportErrorDto
                    {
                        Id = e.Id,
                        FieldName = e.FieldName,
                        ErrorType = e.ErrorType,
                        ErrorMessage = e.ErrorMessage
                    }
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (error == null)
                throw new KeyNotFoundException("Import error not found");

            return error;
        }
    }
}
