using Application.Features.CandDocs.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/importerrors")]
public class ImportErrorsController : ControllerBase
{
    private readonly GetImportErrorsHandler _query;

    public ImportErrorsController(GetImportErrorsHandler query)
    {
        _query = query;
    }

    [HttpGet("import-errors")]
    public async Task<IActionResult> GetImportErrors(
        [FromQuery] int session,
        [FromQuery] string examCode,
        [FromQuery] string? centreCode)
    {
        var result = await _query.HandleAsync(session, examCode, centreCode);
        return Ok(result);
    }
}
