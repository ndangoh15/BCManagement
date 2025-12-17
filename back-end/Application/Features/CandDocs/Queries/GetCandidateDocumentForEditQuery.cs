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
    public class GetCandidateDocumentForEditQuery
    {
        private readonly FsContext _db;

        public GetCandidateDocumentForEditQuery(FsContext db)
        {
            _db = db;
        }

        public async Task<CandidateDocumentEditDto?> ExecuteAsync(int id)
        {
            var doc = await _db.CandidateDocuments
                .Include(d => d.ImportErrors)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null) return null;

            return new CandidateDocumentEditDto
            {
                Id = doc.Id,
                Session = doc.Session,
                ExamCode = doc.ExamCode,
                CentreCode = doc.CentreCode,
                CandidateNumber = doc.CandidateNumber,
                CandidateName = doc.CandidateName,
                FilePath = doc.FilePath,
                PdfUrl = $"/api/files/preview/{doc.Id}",
                Errors = doc.ImportErrors.Select(e => new ImportErrorDto
                {
                    Id = e.Id,
                    FieldName = e.FieldName,
                    ErrorType = e.ErrorType,
                    ErrorMessage = e.ErrorMessage
                }).ToList()
            };
        }
    }

}
