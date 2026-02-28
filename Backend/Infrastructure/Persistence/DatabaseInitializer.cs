using Microsoft.EntityFrameworkCore;
using TestTaskINT20H.Domain.Auth.Entities;
using TestTaskINT20H.Domain.Auth.Services;

namespace TestTaskINT20H.Infrastructure.Persistence;

/// <summary>
/// Applies EF Core migrations for both databases and seeds the default admin account
/// on application startup, before HTTP requests are served.
/// </summary>
public sealed class DatabaseInitializer(
    IServiceScopeFactory scopeFactory,
    IPasswordHasher passwordHasher,
    IConfiguration configuration,
    ILogger<DatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        await MigrateAsync<OrderDbContext>("Orders", scope, cancellationToken);
        await MigrateAsync<AdminDbContext>("Admins", scope, cancellationToken);
        await SeedDefaultAdminAsync(scope, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task MigrateAsync<TContext>(string name, IServiceScope scope, CancellationToken ct)
        where TContext : DbContext
    {
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var pending = await db.Database.GetPendingMigrationsAsync(ct);

        if (!pending.Any())
        {
            logger.LogInformation("{Name} database is up to date.", name);
            return;
        }

        logger.LogInformation("Applying {Count} pending migration(s) to {Name} database...", pending.Count(), name);
        await db.Database.MigrateAsync(ct);
        logger.LogInformation("{Name} database migrations applied.", name);
    }

    private async Task SeedDefaultAdminAsync(IServiceScope scope, CancellationToken ct)
    {
        var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();

        if (await db.Admins.AnyAsync(ct))
            return;

        var password = configuration["DefaultAdmin:Password"];
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException(
                "DefaultAdmin:Password is not configured. Set it via appsettings or the DefaultAdmin__Password environment variable.");

        var username = configuration["DefaultAdmin:Username"] ?? "admin";
        var admin = Admin.Create(username, passwordHasher.Hash(password));
        db.Admins.Add(admin);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Default admin '{Username}' seeded.", username);
    }
}
