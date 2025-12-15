using Microsoft.AspNetCore.Http;

public class UploadDocumentsDto
{
    public List<IFormFile> Files { get; set; } = new();
    public int ExamYear { get; set; }
    public string ExamCode { get; set; }
    public string CenterNumber { get; set; }
    //public int? UploadedBy { get; set; }
}
