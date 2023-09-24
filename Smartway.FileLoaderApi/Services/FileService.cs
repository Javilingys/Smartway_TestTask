using LazyCache;
using Microsoft.EntityFrameworkCore;
using Smartway.FileLoaderApi.Constants;
using Smartway.FileLoaderApi.Contracts;
using Smartway.FileLoaderApi.Entities;
using Smartway.FileLoaderApi.Utils;
using System.IO.Compression;

namespace Smartway.FileLoaderApi.Services;

public class FileService : IFileService
{
    // in kb
    private const int DefaultChunkSize = 16;
    private readonly int _chunkSize;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppCache _memoryCache;
    private readonly IConfiguration _config;

    public FileService(IUnitOfWork unitOfWork, IAppCache memoryCache, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _memoryCache = memoryCache;
        _config = config;

        int chunkSize = _config.GetValue<int>("ChunkSizeInKbs");
        _chunkSize = chunkSize == 0 ? DefaultChunkSize : chunkSize;
    }

    public async Task SaveFilesAsync(IEnumerable<IFormFile> files, string userId, CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[_chunkSize * 1024];
        long totalBytes = files.Sum(x => x.Length);
        int totalLoadPercent = 0;

        long totalUploadedBytes = 0;

        var groupId = Guid.NewGuid();
        foreach (IFormFile formFile in files.Where(x => x.Length > 0))
        {
            var fileNameAndExtension = GetFileNameWithExtension(formFile);
            var fileName = $"{fileNameAndExtension.Name}{fileNameAndExtension.Extension}";
            var fileDirectoryPath = GetPathForFiles();

            DirectoryHelper.CreateDirectoryIfNotExists(fileDirectoryPath);

            var filePath = Path.Combine(fileDirectoryPath, fileName);

            var appFile = new AppFile
            {
                GroupId = groupId,
                AppUserId = userId,
                Extension = fileNameAndExtension.Extension,
                Name = fileNameAndExtension.Name,
                Path = AppConstants.Paths.FileFolderPath,
                SizeInBytes = formFile.Length
            };

            using var output = new FileStream(filePath, FileMode.Create);
            using var input = formFile.OpenReadStream();
            int bytesRead;

            while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await output.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalUploadedBytes += bytesRead;
                totalLoadPercent = (int)((float)totalUploadedBytes / totalBytes * 100.0);

                _memoryCache.Add(userId, totalLoadPercent, TimeSpan.FromSeconds(1));
                // Чтобы замедлить загрузку и можно было успеть посмотреть статус загрузки в постмане
                await Task.Delay(1);
            }

            _unitOfWork.AppFileRepository.Add(appFile);
        }

        await _unitOfWork.SaveChangesAsync();
        _memoryCache.Remove(userId);
    }

    public async Task<string> GenerateZipFileAsync(IEnumerable<AppFile> files)
    {
        // Create a unique ZIP filename (e.g., based on timestamp)
        string zipFileName = $"downloaded_files_{DateTime.Now:yyyyMMddHHmmss}.zip";
        string zipDirectoryPath = GetPathForZips();

        DirectoryHelper.CreateDirectoryIfNotExists(zipDirectoryPath);

        string zipFilePath = Path.Combine(zipDirectoryPath, zipFileName);

        using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
        {
            foreach (var appFile in files)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), appFile.GetFullPath());

                if (System.IO.File.Exists(filePath))
                {
                    archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                }
            }
        }

        return zipFilePath;
    }

    private static (string Name, string Extension) GetFileNameWithExtension(IFormFile file)
    {
        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = Guid.NewGuid().ToString();

        return (Name: fileName, Extension: fileExtension);
    }

    private static string GetPathForFiles()
    {
        return Path.Combine(Directory.GetCurrentDirectory(), AppConstants.Paths.FileFolderPath);
    }

    private static string GetPathForZips()
    {
        return Path.Combine(Directory.GetCurrentDirectory(), AppConstants.Paths.ZipFolderPath);
    }
}
