namespace Smartway.FileLoaderApi.Entities;

public class AppFile
{
    public int Id { get; set; }

    public long SizeInBytes { get; set; }

    public Guid GroupId { get; set; }

    public string Path { get; set; }

    public string Name { get; set; }

    public string Extension { get; set; }

    public AppUser AppUser { get; set; }

    public string AppUserId { get; set; }

    public string GetFullPath()
    {
        return System.IO.Path.Combine(Path, $"{Name}{Extension}");
    }
}
