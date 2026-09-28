namespace Lib.Interfaces;

public interface IFileService
{
    public List<string> GetAllFilePaths(string directoryPath);
    Task WriteAllText(string path, string contents, CancellationToken token);
    Task<string> ReadAllText(string path, CancellationToken token);
    bool Exists(string path);

    public FileStream Open(string path, long? offset, FileMode mode = FileMode.Open,
        FileAccess access = FileAccess.Read);

    public FileStream CreateOrOpen(string path, long? offset = null);
    FileInfo GetFileData(string path);
    DirectoryInfo CreateDirectory(string path);
}
