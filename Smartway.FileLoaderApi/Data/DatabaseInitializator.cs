using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Smartway.FileLoaderApi.Entities;

namespace Smartway.FileLoaderApi.Data;

public static class DatabaseInitializator
{
    private static readonly SemaphoreSlim _migratorEvent = new SemaphoreSlim(0);

    public static async Task Initialize(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        await MigrateAndSeedData(
            scope.ServiceProvider.GetService<AppDbContext>(), 
            scope.ServiceProvider.GetService<UserManager<AppUser>>(),
            scope.ServiceProvider.GetService<ILoggerFactory>());
    }

    private static async Task MigrateAndSeedData(AppDbContext context, UserManager<AppUser> userManager, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(DatabaseInitializator));

        logger.LogInformation("--> Старт миграции..");
        await context.Database.MigrateAsync();
        logger.LogInformation("--> Конец миграции..");

        logger.LogInformation("--> Старт сида данных..");
        if (await userManager.Users.AnyAsync())
        {
            logger.LogInformation("--> Пользователи уже есть в базе.");
        }
        else
        {
            var users = new List<AppUser>
            {
                new AppUser
                {
                    Email = "test@test.com",
                    UserName = "test@test.com",
                },
                new AppUser
                {
                    Email = "test2@test.com",
                    UserName = "test2test.com",
                }
            };

            foreach (var appUser in users)
            {
                await userManager.CreateAsync(appUser, "password");
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("--> Конец сида данных..");
        _migratorEvent.Release(1);
    }

    public static Task WaitForMigrationAsync(CancellationToken cancellationToken = default)
    {
        return _migratorEvent.WaitAsync(cancellationToken);
    }
}
