using Application.Features.CandDocs.Queries;
using Domain.DTO.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/candidate-documents")]
public class CandidateDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CandidateDocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search(
        [FromBody] SearchCandidateDocumentsRequest request)
    {
        // 👉 FluentValidation s’exécute automatiquement ici
        // Si invalide → 400 BadRequest

        var query = new SearchDocumentQuery
        {
            CandidateName = request.CandidateName,
            CandidateNumber = request.CandidateNumber,
            CenterNumber = request.CenterNumber,
            ExamCode = request.ExamCode,
            Session = request.Session,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
