using Application.Features.CandDocs.Commands;
using Domain.DTO.CandDocs;
using Domain.InterfacesServices.CandDocs;
using Domain.InterfacesStores.CandDocs;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly UploadBatchHandler _uploadBatchHandler;
        private readonly ICandidateRepository _candidateRepo;
        private readonly IFileStore _fileStore;
        private readonly ITesseractService _ocr;

        public DocumentController(
            UploadBatchHandler uploadBatchHandler,
            ICandidateRepository candidateRepo,
            IFileStore fileStore,
            ITesseractService ocr)
        {
            _uploadBatchHandler = uploadBatchHandler;
            _candidateRepo = candidateRepo;
            _fileStore = fileStore;
            _ocr = ocr;
        }

        // ------------------------------------------------------------
        // 1. UPLOAD BATCH PDF
        // ------------------------------------------------------------
        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocuments([FromForm] UploadBatchRequestDTO request)
        {
            if (request.PdfFile == null || request.PdfFile.Length == 0)
                return BadRequest("No PDF uploaded.");

            // Save uploaded temp file
            var uploadTempPath = Path.Combine(Path.GetTempPath(),
                $"{Guid.NewGuid()}_{request.PdfFile.FileName}");

            using (var fs = new FileStream(uploadTempPath, FileMode.Create, FileAccess.Write))
                await request.PdfFile.CopyToAsync(fs);

            var bytes = await System.IO.File.ReadAllBytesAsync(uploadTempPath);
            var cmd = new UploadBatchCommand(request, bytes, request.UploadedBy)
            {
                ServerSourceFilePath = uploadTempPath
            };

            var result = await _uploadBatchHandler.HandleAsync(cmd);

            return Ok(result);
        }

        // ------------------------------------------------------------
        // 2. GET DOCUMENTS BY CENTRE + SESSION
        // ------------------------------------------------------------
        [HttpGet("list")]
        public async Task<IActionResult> GetDocuments(string session, string exam, string centre)
        {
            var docs = await _candidateRepo.GetDocumentsAsync(session, exam, centre);
            return Ok(docs);
        }

        // ------------------------------------------------------------
        // 3. UPDATE CANDIDATE INFO (Correct validation errors)
        // ------------------------------------------------------------
        [HttpPost("update-candidate")]
        public async Task<IActionResult> UpdateCandidate([FromBody] UpdateCandidateDto dto)
        {
            var doc = await _candidateRepo.GetDocumentByIdAsync(dto.DocumentId);
            if (doc == null) return NotFound("Document not found.");

            doc.CandidateNumber = dto.CandidateNumber;
            doc.CandidateName = dto.CandidateName;
            doc.CentreCode = dto.CentreCode;
            doc.IsValid = true;

            await _candidateRepo.UpdateAsync(doc);

            // MOVE error → success
            if (doc.FilePath.Contains("\\errors\\"))
                doc.FilePath = await _fileStore.MoveToSuccessFolderAsync(doc.FilePath);

            return Ok(new { success = true, message = "Candidate updated successfully." });
        }

        // ------------------------------------------------------------
        // 4. MARK DOCUMENT AS INVALID (Admin correction)
        // ------------------------------------------------------------
        [HttpPost("mark-invalid")]
        public async Task<IActionResult> MarkInvalid([FromBody] MarkInvalidDto dto)
        {
            var doc = await _candidateRepo.GetDocumentByIdAsync(dto.DocumentId);
            if (doc == null) return NotFound();

            doc.IsValid = false;
            await _candidateRepo.UpdateAsync(doc);

            doc.FilePath = await _fileStore.MoveToErrorFolderAsync(doc.FilePath);

            return Ok(new { success = true });
        }

        // ------------------------------------------------------------
        // 5. RE-RUN OCR ON ONE DOCUMENT (optional)
        // ------------------------------------------------------------
        [HttpPost("rerun-ocr")]
        public async Task<IActionResult> ReRunOcr([FromBody] ReRunOcrDto dto)
        {
            var doc = await _candidateRepo.GetDocumentByIdAsync(dto.DocumentId);
            if (doc == null) return NotFound("Document missing.");

            var bytes = await System.IO.File.ReadAllBytesAsync(doc.FilePath);
            var ocr = await _ocr.ExtractTextFromPdfAsync(bytes, 1);

            doc.OcrText = ocr;
            await _candidateRepo.UpdateAsync(doc);

            return Ok(new
            {
                success = true,
                ocr
            });
        }

        // ------------------------------------------------------------
        // 6. DOWNLOAD THE DOCUMENT
        // ------------------------------------------------------------
        [HttpGet("download")]
        public async Task<IActionResult> Download(int id)
        {
            var doc = await _candidateRepo.GetDocumentByIdAsync(id);
            if (doc == null) return NotFound();

            var bytes = await System.IO.File.ReadAllBytesAsync(doc.FilePath);
            var fileName = Path.GetFileName(doc.FilePath);

            return File(bytes, "application/pdf", fileName);
        }
    }
}
