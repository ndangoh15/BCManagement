using Domain.DTO.ImportErrors;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CandDocs.Queries
{
    public class GetImportErrorsHandler
    {
        private readonly FsContext _db;

        public GetImportErrorsHandler(FsContext db)
        {
            _db = db;
        }

        public async Task<List<ImportErrorListDto>> HandleAsync(
            int session,
            string examCode,
            string? centreCode)
        {
            var query = _db.ImportErrors
                .Include(e => e.CandidateDocument)
                .Where(e =>
                    e.Session == session &&
                    e.CandidateDocument.ExamCode == examCode &&
                    !e.CandidateDocument.IsValid
                );

            if (!string.IsNullOrWhiteSpace(centreCode))
            {
                query = query.Where(e =>
                    e.CandidateDocument.FormCentreCode == centreCode);
            }

            return await query
                .OrderByDescending(e => e.ImportDate)
                .Select(e => new ImportErrorListDto
                {
                    ErrorId = e.Id,
                    DocumentId = e.CandidateDocumentId ?? 0,

                    Session = e.Session ?? 0,
                    ExamCode = e.CandidateDocument.ExamCode,
                    CentreCode = e.CandidateDocument.FormCentreCode,

                    CandidateNumber = e.CandidateNumber,
                    CandidateName = e.CandidateName,

                    FieldName = e.FieldName,
                    ErrorType = e.ErrorType,
                    ErrorMessage = e.ErrorMessage,

                    FilePath = e.FilePath,
                    ImportDate = e.ImportDate
                })
                .ToListAsync();
        }
    }

}
