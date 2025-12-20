using Application.Features.CandDocs.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

[Authorize]
[ApiController]
[Route("api/importerrors")]
public class ImportErrorsController : ControllerBase
{
    private readonly GetImportErrorsHandler _query;
    private readonly GetInvalidCentreCodesQuery _handler;

    public ImportErrorsController(GetImportErrorsHandler query, GetInvalidCentreCodesQuery handle)
    {
        _query = query;
        _handler = handle;
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

    [HttpGet("centres")]
    public async Task<IActionResult> GetInvalidCentres(
    [FromQuery] int session,
    [FromQuery] string examCode)
    {
        var centres = await _handler.GetInvalidCentreCodesAsync(session, examCode);
        return Ok(centres);
    }

}
