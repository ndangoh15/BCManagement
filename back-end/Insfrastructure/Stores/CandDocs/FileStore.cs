using Domain.InterfacesStores.CandDocs;
using Microsoft.Extensions.Configuration;

public class FileStore : IFileStore
{
    private readonly string _root;

    public FileStore(IConfiguration config)
    {
        _root = config["StorageRoot"]!;
    }

    private string Ensure(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return path;
    }

    public string BuildBasePath(string s, string e, string c)
        => Path.Combine(_root, s, e, c);

    public string GetImportedFolder(string s, string e, string c)
        => Ensure(Path.Combine(_root, s, e, c, "imported"));

    public string GetSuccessFolder(string s, string e, string c)
        => Ensure(Path.Combine(_root, s, e, c, "success"));

    public string GetErrorFolder(string s, string e, string c)
        => Ensure(Path.Combine(_root, s, e, c, "errors"));

    public async Task<string> SaveSuccessFileAsync(byte[] data, string s, string e, string c, string name)
    {
        string f = GetSuccessFolder(s, e, c);
        string p = Path.Combine(f, name);
        await File.WriteAllBytesAsync(p, data);
        return p;
    }

    public async Task<string> MoveOriginalImportedPdfAsync(byte[] data, string s, string e, string c, string name)
    {
        string f = GetImportedFolder(s, e, c);
        string p = Path.Combine(f, name);
        await File.WriteAllBytesAsync(p, data);
        return p;
    }

    public Task<string> MoveToErrorFolderAsync(string currentPath)
    {
        string newPath = currentPath.Replace("\\success\\", "\\errors\\");
        Ensure(Path.GetDirectoryName(newPath)!);

        if (File.Exists(newPath)) File.Delete(newPath);
        File.Move(currentPath, newPath);

        return Task.FromResult(newPath);
    }

    public Task<string> MoveToSuccessFolderAsync(string currentPath)
    {
        string newPath = currentPath.Replace("\\errors\\", "\\success\\");
        Ensure(Path.GetDirectoryName(newPath)!);

        if (File.Exists(newPath)) File.Delete(newPath);
        File.Move(currentPath, newPath);

        return Task.FromResult(newPath);
    }

    public Task<bool> FileExistsAsync(string path)
        => Task.FromResult(File.Exists(path));
}
