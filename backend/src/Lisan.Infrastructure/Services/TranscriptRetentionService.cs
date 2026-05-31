using Lisan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lisan.Infrastructure.Services;

public sealed class TranscriptRetentionService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<TranscriptRetentionService> logger) : BackgroundService
{
    private const int DefaultRetentionDays = 90;
    private const string RetentionDaysConfigKey = "TRANSCRIPT_RETENTION_DAYS";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Transcript retention service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeUntilNextMidnightUtc(), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (stoppingToken.IsCancellationRequested)
                break;

            await RunCleanupAsync(stoppingToken);
        }

        logger.LogInformation("Transcript retention service stopped");
    }

    internal async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var retentionDays = GetRetentionDays();
            var cutoff = DateTimeOffset.UtcNow - TimeSpan.FromDays(retentionDays);

            var expiredTranscripts = await db.Transcripts
                .Where(t => t.CreatedAt < cutoff)
                .ToListAsync(cancellationToken);

            if (expiredTranscripts.Count == 0)
            {
                logger.LogInformation(
                    "Transcript retention check: no records older than {RetentionDays} days",
                    retentionDays);
                return;
            }

            db.Transcripts.RemoveRange(expiredTranscripts);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Deleted {Count} transcript record(s) older than {RetentionDays} days (cutoff: {Cutoff:O})",
                expiredTranscripts.Count, retentionDays, cutoff);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown — no action needed.
        }
        catch (Exception ex)
        {
            // LogError is forwarded to Sentry via Sentry.Extensions.Logging (registered by Sentry.AspNetCore).
            logger.LogError(ex, "Transcript retention cleanup failed");
        }
    }

    private int GetRetentionDays()
    {
        var raw = configuration[RetentionDaysConfigKey];
        if (int.TryParse(raw, out var days) && days > 0)
            return days;

        return DefaultRetentionDays;
    }

    private static TimeSpan TimeUntilNextMidnightUtc()
    {
        var now = DateTimeOffset.UtcNow;
        var nextMidnight = now.UtcDateTime.Date.AddDays(1);
        return nextMidnight - now.UtcDateTime;
    }
}
