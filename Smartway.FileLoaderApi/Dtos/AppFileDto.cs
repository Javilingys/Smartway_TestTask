using Smartway.FileLoaderApi.Entities;

namespace Smartway.FileLoaderApi.Dtos;

public class AppFileDto
{
    public int Id { get; set; }

    public long SizeInBytes { get; set; }

    public Guid GroupId { get; set; }

    public string Path { get; set; }

    public string FullName { get; set; }
}
