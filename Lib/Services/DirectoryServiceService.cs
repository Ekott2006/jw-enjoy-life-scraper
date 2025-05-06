using Lib.Interfaces;

namespace Lib.Services;

public class DirectoryServiceService : IDirectoryService
{
    public DirectoryInfo CreateDirectory(string path)
    {
        return Directory.CreateDirectory(path);
    }
}