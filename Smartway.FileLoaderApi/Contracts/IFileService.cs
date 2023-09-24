using Smartway.FileLoaderApi.Entities;

namespace Smartway.FileLoaderApi.Contracts;

public interface IFileService
{
    Task SaveFilesAsync(IEnumerable<IFormFile> files, string userId, CancellationToken cancellationToken);

    Task<string> GenerateZipFileAsync(IEnumerable<AppFile> files);
}
