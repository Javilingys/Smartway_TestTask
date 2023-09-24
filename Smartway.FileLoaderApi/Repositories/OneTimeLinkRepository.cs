using Microsoft.EntityFrameworkCore;
using Smartway.FileLoaderApi.Contracts;
using Smartway.FileLoaderApi.Data;
using Smartway.FileLoaderApi.Entities;

namespace Smartway.FileLoaderApi.Repositories;

public class OneTimeLinkRepository : IOneTimeLInkRepository
{
    private readonly AppDbContext _context;

    public OneTimeLinkRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(OneTimeLink oneTimeLink)
    {
        _context.OneTimeLinks.Add(oneTimeLink);
    }

    public async Task<OneTimeLink> GetActiveByToken(string token, bool asNoTracking = false)
    {
        return await AppFileQuery(asNoTracking)
            .Where(x => !x.WasUsed &&
                        x.Expiry > DateTime.UtcNow &&
                        x.Token == token)
            .FirstOrDefaultAsync();
    }

    private IQueryable<OneTimeLink> AppFileQuery(bool asNoTracking)
    {
        return asNoTracking
            ? _context.OneTimeLinks.AsNoTracking().AsQueryable()
            : _context.OneTimeLinks.AsQueryable();
    }
}
