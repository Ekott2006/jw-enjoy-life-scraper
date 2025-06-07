namespace Lib.Interfaces;

public interface IFileService
{
    Task WriteAllText(string path, string contents, CancellationToken token);
    Task<string> ReadAllText(string path, CancellationToken token);
    bool Exists(string path);
    FileStream Create(string path);
    FileInfo GetFileData(string path);
    DirectoryInfo CreateDirectory(string path);
    
}