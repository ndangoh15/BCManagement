using Domain.DTO.CandDocs;
using Domain.Entities.CandDocs;
using Domain.InterfacesServices.CandDocs;
using Domain.InterfacesStores.CandDocs;
using Domain.Models.CandDocs;

public class UploadBatchHandler : IRequestHandler<UploadBatchRequestDTO, UploadBatchResultDTO>
{
    private readonly IPdfSplitService _pdfService;                 // For splitting pages
    private readonly ITesseractService _ocr;                 // OCR service
    private readonly ICandidateRepository _repo;            // Save DB records
    private readonly IFileStore _fileStore;                 // Folder management
    private readonly IDocumentValidator _validator;         // Validation rules
    private readonly string _uploadedBy;

    public UploadBatchHandler(
        IPdfSplitService pdfService,
        ITesseractService ocr,
        ICandidateRepository repo,
        IFileStore fileStore,
        IDocumentValidator validator)
    {
        _pdfService = pdfService;
        _ocr = ocr;
        _repo = repo;
        _fileStore = fileStore;
        _validator = validator;
        _uploadedBy = "system"; // Replace with real user when available
    }

    public async Task<UploadBatchResultDTO> Handle(UploadBatchRequestDTO command, CancellationToken cancellationToken)
    {
        var result = new UploadBatchResultDTO();
        result.SavedFilePaths = new List<string>();

        string sessionStr = command.ExamYear.ToString();
        string examStr = command.ExamCode.ToString();
        string centreStr = command.CentreNumber.ToString();

        // --------------------------------------------------------------------
        // STEP 1: READ ORIGINAL FILE INTO MEMORY (100% prevents file locking)
        // --------------------------------------------------------------------
        if (!File.Exists(command.ServerSourceFilePath))
            throw new FileNotFoundException("Source file does not exist.", command.ServerSourceFilePath);

        byte[] originalBytes = await File.ReadAllBytesAsync(command.ServerSourceFilePath);

        // --------------------------------------------------------------------
        // STEP 2: SPLIT PDF INTO PAGES (memory-only)
        // --------------------------------------------------------------------
        var pages = await _pdfService.SplitPdfByPageAsync(originalBytes);
        if (pages.Count == 0)
            throw new Exception("No PDF pages found.");

        // --------------------------------------------------------------------
        // STEP 3: PROCESS EACH DOCUMENT (one document = page1 + page2)
        // --------------------------------------------------------------------
        int docIndex = 0;

        for (int i = 0; i < pages.Count; i += 2)
        {
            byte[] page1 = pages[i];
            byte[] page2 = (i + 1 < pages.Count ? pages[i + 1] : null);

            // ----------------------------------------------------------------
            // EXTRACT INFORMATION VIA OCR
            // ----------------------------------------------------------------
            string textPage1 = await _ocr.ExtractTextFromPdfAsync(page1, 1);
            string textPage2 = page2 != null
                ? await _ocr.ExtractTextFromPdfAsync(page2, 1)
                : "";

            string ocrCombined = textPage1 + "\n" + textPage2;

            // Candidate info extraction  
            CandidateInfo info = CandidateInfoExtractor.Extract(ocrCombined);

            // ----------------------------------------------------------------
            // VALIDATE DOCUMENT
            // ----------------------------------------------------------------
            string tempSavedFilePath = "(not saved yet)";
            var validation = _validator.Validate(page1, ocrCombined, info, tempSavedFilePath, command);

            bool isValid = !validation.Errors.Any();

            // ----------------------------------------------------------------
            // SAVE FILE IN SUCCESS OR ERROR FOLDER
            // ----------------------------------------------------------------
            string newPdfName = BuildDocumentFileName(command, info, docIndex);

            if (isValid)
            {
                // Merge pages (in memory)
                byte[] merged = await _pdfService.MergeTwoPagesAsync(page1, page2);

                string savedPath = await _fileStore.SaveSuccessFileAsync(
                    merged, sessionStr, examStr, centreStr, newPdfName);

                tempSavedFilePath = savedPath;
                result.SavedFilePaths.Add(savedPath);
            }
            else
            {
                // Only page1 is stored; page2 is irrelevant for error
                string savedPath = await _fileStore.SaveErrorFileAsync(
                    page1, sessionStr, examStr, centreStr, newPdfName);

                tempSavedFilePath = savedPath;
                result.SavedFilePaths.Add(savedPath);
            }

            // ----------------------------------------------------------------
            // SAVE DATABASE RECORD
            // ----------------------------------------------------------------
            var candDoc = new CandidateDocument
            {
                Session = command.ExamYear,
                CentreCode = command.CentreNumber,
                CandidateNumber = info.CandidateNumber ?? "",
                CandidateName = info.CandidateName ?? "",
                OcrText = ocrCombined,
                FilePath = tempSavedFilePath,
                IsValid = isValid,
                UploadedBy = _uploadedBy,
                FormCentreCode = info.FormCentreCode
            };

            await _repo.AddAsync(candDoc);

            // Save validation errors in DB
            foreach (var err in validation.Errors)
                err.CandidateDocumentId = candDoc.Id;

            if (validation.Errors.Any())
                await _repo.AddImportErrorsAsync(validation.Errors);

            docIndex++;
        }

        // --------------------------------------------------------------------
        // STEP 4: SAVE A COPY IN IMPORTED FOLDER (with _Tr suffix)
        // --------------------------------------------------------------------
        string originalName = Path.GetFileNameWithoutExtension(command.ServerSourceFilePath);
        string originalExt = Path.GetExtension(command.ServerSourceFilePath);

        string importedName = await GenerateUniqueImportedName(
            sessionStr, examStr, centreStr, originalName, originalExt);

        string importedPath = await _fileStore.MoveOriginalImportedPdfAsync(
            originalBytes, sessionStr, examStr, centreStr, importedName);

        result.ImportedPath = importedPath;

        // --------------------------------------------------------------------
        // STEP 5: DELETE ORIGINAL SOURCE FILE (NOW SAFE!)
        // --------------------------------------------------------------------
        if (File.Exists(command.ServerSourceFilePath))
            File.Delete(command.ServerSourceFilePath);

        result.TotalCandidates = result.SavedFilePaths.Count;
        return result;
    }

    // ------------------------------------------------------------------------
    // UTILITY: UNIQUE IMPORTED FILENAME
    // ------------------------------------------------------------------------
    private async Task<string> GenerateUniqueImportedName(string session, string exam, string centre, string baseName, string ext)
    {
        string folder = _fileStore.GetImportedFolder(session, exam, centre);

        string name = $"{baseName}_Tr{ext}";
        string path = Path.Combine(folder, name);

        int counter = 1;
        while (File.Exists(path))
        {
            name = $"{baseName}_Tr{counter}{ext}";
            path = Path.Combine(folder, name);
            counter++;
        }

        return name;
    }

    // ------------------------------------------------------------------------
    // UTILITY: FINAL FILE NAME FOR EACH DOCUMENT
    // ------------------------------------------------------------------------
    private string BuildDocumentFileName(UploadBatchRequestDTO command, CandidateInfo info, int index)
    {
        string cnum = string.IsNullOrWhiteSpace(info.CandidateNumber)
            ? $"UNK{index}"
            : info.CandidateNumber;

        return $"{command.CentreNumber}_{command.ExamYear}_{cnum}.pdf";
    }
}
