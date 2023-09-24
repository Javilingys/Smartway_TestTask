using Microsoft.EntityFrameworkCore;
using Smartway.FileLoaderApi.Data;

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
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessCleanAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ProcessCleanAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--> Началась очистка использованных ссылок.");

        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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
