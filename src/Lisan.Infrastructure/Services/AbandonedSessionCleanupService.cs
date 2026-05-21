using Lisan.Domain.Enums;
using Lisan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lisan.Infrastructure.Services;

public sealed class AbandonedSessionCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<AbandonedSessionCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMinutes(5);
private static readonly TimeSpan InactivityThreshold = TimeSpan.FromMinutes(10);

protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    logger.LogInformation("Abandoned session cleanup service started");

    while (!stoppingToken.IsCancellationRequested)
    {
        await Task.Delay(PollingInterval, stoppingToken);

        if (stoppingToken.IsCancellationRequested)
            break;

        await RunCleanupAsync(stoppingToken);
    }

    logger.LogInformation("Abandoned session cleanup service stopped");
}

public async Task RunCleanupAsync(CancellationToken cancellationToken)
{
    try
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cutoff = DateTimeOffset.UtcNow - InactivityThreshold;

        var staleSessions = await db.Sessions
            .Where(s => s.Status == SessionStatus.Active && s.LastActivityAt < cutoff)
            .ToListAsync(cancellationToken);

        if (staleSessions.Count == 0)
            return;

        var abandonedAt = DateTimeOffset.UtcNow;
        foreach (var session in staleSessions)
            session.Abandon(abandonedAt);

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Abandoned {Count} stale session(s) with last activity before {Cutoff:O}",
            staleSessions.Count, cutoff);
    }
    catch (OperationCanceledException)
    {
        // Normal shutdown — no action needed.
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during abandoned session cleanup");
    }
}
}
