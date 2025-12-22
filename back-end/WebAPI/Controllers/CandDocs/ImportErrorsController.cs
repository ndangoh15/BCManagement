using Application.Features.CandDocs.Queries;
using Application.Features.ImportErrors.Commands;
using MediatR;
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
    private readonly IMediator _mediator;

    public ImportErrorsController(GetImportErrorsHandler query, GetInvalidCentreCodesQuery handle, IMediator mediator)
    {
        _query = query;
        _handler = handle;
        _mediator = mediator;
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetErrorDetail(int id)
    {
        var result = await _mediator.Send(new GetImportErrorDetailQuery(id));
        return Ok(result);
    }

    [HttpPost("fix")]
    public async Task<IActionResult> FixImportError([FromBody] FixImportErrorCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }

}
