using Application.Features.CandDocs.Commands;
using Application.Features.CandDocs.Queries;
using Domain.DTO.CandDocs;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/document")]
    public class DocumentController : ControllerBase
    {
        private readonly UploadBatchHandler _handler;
        private readonly IConfiguration _config;
        private readonly GetImportedBatchesHandler _checkHandler;
        public DocumentController(IConfiguration config, UploadBatchHandler handler, GetImportedBatchesHandler checkHandler)
        {
            _handler = handler;
            _config = config;
            _checkHandler = checkHandler;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadFormDto form)
        {
            if (form.File == null || form.File.Length == 0) return BadRequest("File is required.");

           

            // save temp file
            var tmp = Path.Combine(Path.GetTempPath(), $"{form.File.FileName}");
            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write))
                await form.File.CopyToAsync(fs);


            // overwrite if previously imported
            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write))
                await form.File.CopyToAsync(fs);

            var dto = new UploadBatchRequestDTO
            {
                ServerSourceFilePath = tmp,
                ExamYear = form.ExamYear,
                ExamCode = form.ExamCode,
                CenterNumber = form.CenterNumber,
                UploadedBy = form.UploadedBy
            };

            var res = await _handler.HandleAsync(dto);
            return Ok(res);
        }

        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultiple([FromForm] UploadDocumentsDto form)
        {
            if (form.Files == null || form.Files.Count == 0)
                return BadRequest("Please select at least one PDF file.");

            string uploadFolder = Path.Combine("Uploads", form.ExamYear.ToString(), form.ExamCode, form.CenterNumber);
            Directory.CreateDirectory(uploadFolder);

            List<string> serverFilePaths = new();

            foreach (var file in form.Files)
            {
                string filePath = Path.Combine(uploadFolder, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                serverFilePaths.Add(filePath);
            }

            var results = await _handler.HandleMultipleFilesAsync(
                serverFilePaths,
                form.ExamYear,
                form.ExamCode,
                form.CenterNumber,
                form.UploadedBy ?? 2
            );

            return Ok(new
            {
                message = $"{results.Count} files processed",
                results
            });
        }

        [HttpGet("imported-batches")]
        public async Task<IActionResult> GetImportedBatches(
            [FromServices] GetImportedBatchesHandler handler)
        {
            var batches = await handler.HandleAsync();
            return Ok(batches);
        }

        [HttpPost("check-exists")]
        public async Task<IActionResult> CheckIfAlreadyImported([FromBody] List<string> fileNames)
        {
            var existing = await _checkHandler.HandleImportedFilesAsync(fileNames);
            return Ok(existing);
        }

    }
}
