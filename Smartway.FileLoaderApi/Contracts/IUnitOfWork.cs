namespace Smartway.FileLoaderApi.Contracts;

public interface IUnitOfWork
{
    IAppFileRepository AppFileRepository { get; }
    IOneTimeLInkRepository OneTimeLInkRepository { get; }

    Task SaveChangesAsync();
}
