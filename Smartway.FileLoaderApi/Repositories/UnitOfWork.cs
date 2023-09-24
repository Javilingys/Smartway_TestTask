using Smartway.FileLoaderApi.Contracts;
using Smartway.FileLoaderApi.Data;

namespace Smartway.FileLoaderApi.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private AppFileRepository _fileRepository;
    private OneTimeLinkRepository _oneTimeLinkRepository;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IAppFileRepository AppFileRepository => _fileRepository ??= new AppFileRepository(_context);

    public IOneTimeLInkRepository OneTimeLInkRepository => _oneTimeLinkRepository ??= new OneTimeLinkRepository(_context);

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
