using Application.Features.CandDocs.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/candidate-documents")]
public class CandidateDocumentsController : ControllerBase
{
    private readonly GetCandidateDocumentForEditQuery _query;

    public CandidateDocumentsController(GetCandidateDocumentForEditQuery query)
    {
        _query = query;
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> GetForEdit(int id)
    {
        var result = await _query.ExecuteAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
