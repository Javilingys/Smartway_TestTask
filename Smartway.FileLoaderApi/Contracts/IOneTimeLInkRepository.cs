using Smartway.FileLoaderApi.Entities;

namespace Smartway.FileLoaderApi.Contracts;

public interface IOneTimeLInkRepository
{
    Task<OneTimeLink> GetActiveByToken(string token, bool asNoTracking = false);
    void Add(OneTimeLink oneTimeLink);
}
