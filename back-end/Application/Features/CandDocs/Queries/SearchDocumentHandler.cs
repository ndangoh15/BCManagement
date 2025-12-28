using Application.Common.Models;
using Application.Features.CandDocs.Queries;
using Domain.DTO.CandDocs;
using Domain.InterfacesStores.CandDocs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class SearchDocumentHandler  : IRequestHandler<SearchDocumentQuery, PagedResult<CandidateDocumentDto>>
{
    private readonly ICandidateRepository _repository;

    public SearchDocumentHandler(ICandidateRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<CandidateDocumentDto>> Handle(
        SearchDocumentQuery query,
        CancellationToken cancellationToken)
    {
        // 1️⃣ Base query (filtrée côté repository)
        var baseQuery = _repository.Search(
            query.CandidateName,
            query.CandidateNumber,
            query.CenterNumber,
            query.ExamCode,
            query.Session
        );

        // 2️⃣ Total pour AG-Grid (obligatoire)
        var totalCount = await baseQuery.CountAsync(cancellationToken);

        // 3️⃣ Page demandée
        var items = await baseQuery
            .OrderBy(x => x.CandidateName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new CandidateDocumentDto
            {
                Id = x.Id,
                CandidateNumber = x.CandidateNumber ?? string.Empty,
                CandidateName = x.CandidateName ?? string.Empty,
                CentreCode = x.CentreCode ?? string.Empty,
                ExamCode = x.ExamCode ?? string.Empty,
                Session = x.Session,
                FilePath = x.FilePath
            })
            .ToListAsync(cancellationToken);

        // 4️⃣ Résultat paginé
        return new PagedResult<CandidateDocumentDto>
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}
