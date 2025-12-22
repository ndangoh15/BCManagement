using Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly FsContext _db;
        private readonly ILogger<FilesController> _logger;

        public FilesController(FsContext db, ILogger<FilesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("preview/{documentId}")]
        public async Task<IActionResult> PreviewPdf(int documentId)
        {
            var doc = await _db.CandidateDocuments.FindAsync(documentId);

            if (doc == null)
                return NotFound("Document not found");

            if (string.IsNullOrWhiteSpace(doc.FilePath))
                return BadRequest("File path not defined");

            if (!System.IO.File.Exists(doc.FilePath))
            {
                _logger.LogWarning("PDF file not found: {Path}", doc.FilePath);
                return NotFound("PDF file not found");
            }

            var stream = new FileStream(
                doc.FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );
            return File(stream, "application/pdf", enableRangeProcessing: true);

        }
    }
}
