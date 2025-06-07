using Lib.Interfaces;

namespace Lib.Services;

public class FileService : IFileService
{
    public async Task WriteAllText(string path, string contents, CancellationToken token = default)
    {
        await File.WriteAllTextAsync(path, contents, token);
    }

    public async Task<string> ReadAllText(string path, CancellationToken token = default)
    {
        return await File.ReadAllTextAsync(path, token);
    }

    public FileInfo GetFileData(string path)
    {
        return new FileInfo(path);
    }

    public bool Exists(string path)
    {
        return File.Exists(path);
    }

    public FileStream Create(string path)
    {
        return File.Create(path);
    }

     public DirectoryInfo CreateDirectory(string path)
    {
        return Directory.CreateDirectory(path);
    }
}