using Application.Features.CandDocs.Commands;
using Domain.DTO.CandDocs;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly UploadBatchHandler _handler;
        private readonly IConfiguration _config;
        public DocumentController(IConfiguration config, UploadBatchHandler handler)
        {
            _handler = handler;
            _config = config;
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

        
    }
}
