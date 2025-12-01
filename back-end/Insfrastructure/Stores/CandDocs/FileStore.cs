using Domain.InterfacesStores.CandDocs;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;


public class FileStore : IFileStore
{
    public string RootPath { get; }

    public FileStore(string rootPath)
    {
        RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
    }

    private string EnsureDir(string folder)
    {
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        return folder;
    }

    private string BuildBase(string session, string exam, string centre)
        => EnsureDir(Path.Combine(RootPath, session, exam, centre));

    public string GetImportedFolder(string session, string exam, string centre)
        => EnsureDir(Path.Combine(RootPath, session, exam, centre, "imported"));

    public async Task<string> SaveSuccessFileAsync(byte[] bytes, string session, string exam, string centre, string fileName)
    {
        var basePath = BuildBase(session, exam, centre);
        var folder = EnsureDir(Path.Combine(basePath, "success"));
        var path = Path.Combine(folder, fileName);
        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            await fs.WriteAsync(bytes, 0, bytes.Length);
        return path;
    }

    public async Task<string> SaveErrorFileAsync(byte[] bytes, string session, string exam, string centre, string fileName)
    {
        var basePath = BuildBase(session, exam, centre);
        var folder = EnsureDir(Path.Combine(basePath, "errors"));
        var path = Path.Combine(folder, fileName);
        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            await fs.WriteAsync(bytes, 0, bytes.Length);
        return path;
    }

    public async Task<string> MoveOriginalImportedPdfAsync(byte[] bytes, string session, string exam, string centre, string fileName)
    {
        var folder = GetImportedFolder(session, exam, centre);
        var path = Path.Combine(folder, fileName);
        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            await fs.WriteAsync(bytes, 0, bytes.Length);
        return path;
    }

    public Task<string> MoveToErrorFolderAsync(string currentPath)
    {
        var newPath = currentPath.Replace(Path.DirectorySeparatorChar + "success" + Path.DirectorySeparatorChar,
                                          Path.DirectorySeparatorChar + "errors" + Path.DirectorySeparatorChar);
        var dir = Path.GetDirectoryName(newPath);
        if (!string.IsNullOrEmpty(dir)) EnsureDir(dir);
        if (File.Exists(newPath)) File.Delete(newPath);
        File.Move(currentPath, newPath);
        return Task.FromResult(newPath);
    }

    public Task<string> MoveToSuccessFolderAsync(string currentPath)
    {
        var newPath = currentPath.Replace(Path.DirectorySeparatorChar + "errors" + Path.DirectorySeparatorChar,
                                          Path.DirectorySeparatorChar + "success" + Path.DirectorySeparatorChar);
        var dir = Path.GetDirectoryName(newPath);
        if (!string.IsNullOrEmpty(dir)) EnsureDir(dir);
        if (File.Exists(newPath)) File.Delete(newPath);
        File.Move(currentPath, newPath);
        return Task.FromResult(newPath);
    }

    public async Task DeleteBatchFolderAsync(string year, string exam, string centre)
    {
        string folder = Path.Combine(RootPath, year, exam, centre);

        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }

        await Task.CompletedTask;
    }

}

