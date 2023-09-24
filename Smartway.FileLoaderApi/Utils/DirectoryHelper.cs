namespace Smartway.FileLoaderApi.Utils;

public static class DirectoryHelper
{
    public static void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
