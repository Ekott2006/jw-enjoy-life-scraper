using Lib.Interfaces;

namespace Lib.Services;

public class CustomDirectory: IDirectory
{
    public DirectoryInfo CreateDirectory(string path)
    {
        return Directory.CreateDirectory(path);
    }
}