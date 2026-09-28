using Lib.Interfaces;

namespace Lib.Services;

public class FileService : IFileService
{
    public List<string> GetAllFilePaths(string directoryPath)
    {
        return [.. Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories)];
    }

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

    public FileStream CreateOrOpen(string path, long? offset = null)
    {
        return Open(
            path,
            mode: (offset ?? 0) > 0 ? FileMode.OpenOrCreate : FileMode.Create,
            access: FileAccess.Write,
            offset: offset
        );
    }

    public FileStream Open(string path, long? offset = null, FileMode mode = FileMode.Open,
        FileAccess access = FileAccess.Read)
    {
        FileStream stream = new(path, mode, access, FileShare.Read);
        stream.Position = offset ?? 0;
        return stream;
    }

    public DirectoryInfo CreateDirectory(string path)
    {
        return Directory.CreateDirectory(path);
    }
}
