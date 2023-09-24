using Smartway.FileLoaderApi.Dtos;
using Smartway.FileLoaderApi.Entities;

namespace Smartway.FileLoaderApi.Contracts;

public interface IAppFileRepository
{
    Task<List<AppFileDto>> GetListOfLoadedFileDtosAsync(string userId, bool canSeeAll, bool asNoTracking = false);
    Task<List<AppFile>> GetFilesByGroupOrFileIdForUser(Guid? groupId, int? fileId, string userId, bool asNoTracking = false);
    Task<List<AppFile>> GetFilesByGroupId(Guid groupId, bool asNoTracking = false);

    void Add(AppFile appFile);
}
