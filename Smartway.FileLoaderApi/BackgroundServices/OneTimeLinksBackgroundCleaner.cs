using Microsoft.EntityFrameworkCore;
using Smartway.FileLoaderApi.Data;
using System.Threading;

namespace Smartway.FileLoaderApi.BackgroundServices;

public class OneTimeLinksBackgroundCleaner : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OneTimeLinksBackgroundCleaner> _logger;

    public OneTimeLinksBackgroundCleaner(
        IServiceProvider serviceProvider,
        ILogger<OneTimeLinksBackgroundCleaner> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DatabaseInitializator.WaitForMigrationAsync(stoppingToken);

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessCleanAsync(dbContext, stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(config.GetValue<int>("CleanerIntervalInSeconds")), stoppingToken);
        }
    }

    private async Task ProcessCleanAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation("--> Началась очистка использованных ссылок.");
        
        var uselessLinks = await dbContext.OneTimeLinks
            .Where(x => x.WasUsed || x.Expiry <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (uselessLinks.Count == 0)
        {
            _logger.LogInformation("--> Ссылок для удаления не нашлось.");
            return;
        }

        dbContext.RemoveRange(uselessLinks);
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("--> Закончилась очистка использованных ссылок. Удалено ссылок {count}", uselessLinks.Count);
    }

}
