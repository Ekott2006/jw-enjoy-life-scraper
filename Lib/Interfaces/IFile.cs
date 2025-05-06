namespace Lib.Interfaces;

public interface IFile
{
    Task WriteAllText(string path, string contents, CancellationToken token);
    Task<string> ReadAllText(string path, CancellationToken token);
    bool Exists(string path);
    FileStream Create(string path);
}