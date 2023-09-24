using Microsoft.EntityFrameworkCore;
using Smartway.FileLoaderApi.Contracts;
using Smartway.FileLoaderApi.Data;
using Smartway.FileLoaderApi.Dtos;
using Smartway.FileLoaderApi.Entities;

namespace Smartway.FileLoaderApi.Repositories;

public class AppFileRepository : IAppFileRepository
{
    private readonly AppDbContext _dbContext;

    public AppFileRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Репозитори знает о дто. Можно было бы воспользовать IMapper и его ProjectTo и передавать как дженерик параметр сюда.
    public async Task<List<AppFileDto>> GetListOfLoadedFileDtosAsync(string userId, bool canSeeAll, bool asNoTracking = false)
    {
        return await AppFileQuery(asNoTracking)
            .Where(x => canSeeAll || x.AppUserId == userId)
            .OrderBy(x => x.GroupId)
            .ThenBy(x => x.Name)
            .Select(x => new AppFileDto
            {
                FullName = $"{x.Name}{x.Extension}",
                GroupId = x.GroupId,
                Id = x.Id,
                Path = $"{x.Path}/{x.Name}{x.Extension}",
                SizeInBytes = x.SizeInBytes,
            })
            .ToListAsync();
    }

    public async Task<List<AppFile>> GetFilesByGroupOrFileIdForUser(Guid? groupId, int? fileId, string userId, bool asNoTracking = false)
    {
        return await AppFileQuery(asNoTracking)
            .Where(x => x.AppUserId == userId &&
                ((groupId.HasValue && x.GroupId == groupId) ||
                 (fileId.HasValue && x.Id == fileId)))
            .ToListAsync();
    }

    public async Task<List<AppFile>> GetFilesByGroupId(Guid groupId, bool asNoTracking = false)
    {
        return await AppFileQuery(asNoTracking)
            .AsNoTracking()
            .Where(x => x.GroupId == groupId)
            .ToListAsync();
    }

    public void Add(AppFile file)
    {
        _dbContext.AppFiles.Add(file);
    }

    private IQueryable<AppFile> AppFileQuery(bool asNoTracking)
    {
        return asNoTracking
            ? _dbContext.AppFiles.AsNoTracking().AsQueryable()
            : _dbContext.AppFiles.AsQueryable();
    }
}
