using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Application.Features.CandDocs.Queries
{
    public class GetInvalidCentreCodesQuery
    {
        private readonly FsContext _db;

        public GetInvalidCentreCodesQuery(FsContext db)
        {
            _db = db;
        }
        public async Task<List<string>> GetInvalidCentreCodesAsync(int session, string examCode)
        {
            return await _db.CandidateDocuments
                .Where(d =>
                    !d.IsValid &&
                    d.Session == session &&
                    d.ExamCode == examCode &&
                    !string.IsNullOrEmpty(d.FormCentreCode))
                .Select(d => d.FormCentreCode)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

    }
}
