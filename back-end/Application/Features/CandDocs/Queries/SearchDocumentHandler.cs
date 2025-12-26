using Application.Common.Models;
using Application.Features.CandDocs.Queries;
using Domain.Entities.CandDocs;
using Domain.InterfacesStores.CandDocs;
using Microsoft.EntityFrameworkCore;

namespace BCDocumentManagement.Application.Features.CandDocs.Queries
{
    public class SearchDocumentHandler
    {
        private readonly ICandidateRepository _repo;

        public SearchDocumentHandler(ICandidateRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResult<CandidateDocument>> HandleAsync( SearchDocumentQuery query,    CancellationToken ct)
        {
            if (query.Session <= 0)
                throw new ArgumentException("Session is required");

            var baseQuery = _repo.Search(query.CandidateName,query.CandidateNumber, query.CenterNumber,query.ExamCode,query.Session);

            var total = await baseQuery.CountAsync(ct);

            var items = await baseQuery
                .OrderBy(x => x.CandidateName)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(ct);

            return new PagedResult<CandidateDocument>
            {
                TotalCount = total,
                Items = items
            };
        }

    }
}
