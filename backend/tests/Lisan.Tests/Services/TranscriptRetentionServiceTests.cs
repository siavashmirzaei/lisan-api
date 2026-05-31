using FluentAssertions;
using Lisan.Domain.Entities;
using Lisan.Domain.Enums;
using Lisan.Infrastructure.Persistence;
using Lisan.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lisan.Tests.Services;

public class TranscriptRetentionServiceTests : IDisposable
{
    private readonly InMemoryDatabaseRoot _dbRoot = new();
    private const string DbName = "LisanRetentionTestDb";
    private readonly ServiceProvider _serviceProvider;

    public TranscriptRetentionServiceTests()
    {
        _serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(DbName, _dbRoot))
            .BuildServiceProvider();
    }

    private AppDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(DbName, _dbRoot)
            .Options);

    private static IConfiguration BuildConfig(string? retentionDays = null)
    {
        var values = new Dictionary<string, string?>();
        if (retentionDays is not null)
            values["TRANSCRIPT_RETENTION_DAYS"] = retentionDays;
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private TranscriptRetentionService CreateSut(
        IConfiguration? config = null,
        ILogger<TranscriptRetentionService>? logger = null) =>
        new(
            _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            config ?? BuildConfig(),
            logger ?? NullLogger<TranscriptRetentionService>.Instance);

    private static Transcript CreateTranscriptWithCreatedAt(DateTimeOffset createdAt)
    {
        var transcript = Transcript.Record(Guid.NewGuid(), 1, TranscriptSpeaker.Child, "سلام");
        // CreatedAt is private set; use reflection to backdate for retention testing.
        typeof(Transcript).GetProperty(nameof(Transcript.CreatedAt))!
            .SetValue(transcript, createdAt);
        return transcript;
    }

    [Fact]
    public async Task RunCleanup_DeletesTranscripts_OlderThanDefaultRetentionPeriod()
    {
        var old = CreateTranscriptWithCreatedAt(DateTimeOffset.UtcNow.AddDays(-91));
        var alsoOld = CreateTranscriptWithCreatedAt(DateTimeOffset.UtcNow.AddDays(-365));
        await using (var ctx = CreateContext())
        {
            ctx.Transcripts.AddRange(old, alsoOld);
            await ctx.SaveChangesAsync();
        }

        await CreateSut().RunCleanupAsync(CancellationToken.None);

        await using var readCtx = CreateContext();
        (await readCtx.Transcripts.ToListAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task RunCleanup_PreservesTranscripts_NewerThanRetentionPeriod()
    {
        var recent = CreateTranscriptWithCreatedAt(DateTimeOffset.UtcNow.AddDays(-89));
        var brandNew = CreateTranscriptWithCreatedAt(DateTimeOffset.UtcNow.AddMinutes(-1));
        await using (var ctx = CreateContext())
        {
            ctx.Transcripts.AddRange(recent, brandNew);
            await ctx.SaveChangesAsync();
        }

        await CreateSut().RunCleanupAsync(CancellationToken.None);

        await using var readCtx = CreateContext();
        var remaining = await readCtx.Transcripts.ToListAsync();
        remaining.Should().HaveCount(2);
        remaining.Select(t => t.Id).Should().BeEquivalentTo(new[] { recent.Id, brandNew.Id });
    }

    [Fact]
    public async Task RunCleanup_DeletesOnlyExpiredTranscripts_WhenMixed()
    {
        var expired = CreateTranscriptWithCreatedAt(DateTimeOffset.UtcNow.AddDays(-91));
        var kept = CreateTranscriptWithCreatedAt(DateTimeOffset.UtcNow.AddDays(-30));
        await using (var ctx = CreateContext())
        {
            ctx.Transcripts.AddRange(expired, kept);
            await ctx.SaveChangesAsync();
        }

        await CreateSut().RunCleanupAsync(CancellationToken.None);

        await using var readCtx = CreateContext();
        var remaining = await readCtx.Transcripts.ToListAsync();
        remaining.Should().ContainSingle().Which.Id.Should().Be(kept.Id);
    }

    [Fact]
    public async Task RunCleanup_UsesConfiguredRetention_WhenSet()
    {
        // With 30-day retention, a 60-day-old transcript should be deleted.
        var sixtyDaysOld = CreateTranscriptWithCreatedAt(DateTimeOffset.UtcNow.AddDays(-60));
        var fifteenDaysOld = CreateTranscriptWithCreatedAt(DateTimeOffset.UtcNow.AddDays(-15));
        await using (var ctx = CreateContext())
        {
            ctx.Transcripts.AddRange(sixtyDaysOld, fifteenDaysOld);
            await ctx.SaveChangesAsync();
        }

        await CreateSut(BuildConfig("30")).RunCleanupAsync(CancellationToken.None);

        await using var readCtx = CreateContext();
        var remaining = await readCtx.Transcripts.ToListAsync();
        remaining.Should().ContainSingle().Which.Id.Should().Be(fifteenDaysOld.Id);
    }

    [Fact]
    public async Task RunCleanup_FallsBackToDefault_WhenConfigIsInvalid()
    {
        // Invalid value should fall back to 90-day default — 89-day-old transcript survives.
        var underDefault = CreateTranscriptWithCreatedAt(DateTimeOffset.UtcNow.AddDays(-89));
        await using (var ctx = CreateContext())
        {
            ctx.Transcripts.Add(underDefault);
            await ctx.SaveChangesAsync();
        }

        await CreateSut(BuildConfig("not-a-number")).RunCleanupAsync(CancellationToken.None);

        await using var readCtx = CreateContext();
        (await readCtx.Transcripts.SingleAsync()).Id.Should().Be(underDefault.Id);
    }

    [Fact]
    public async Task RunCleanup_NoExpiredTranscripts_DoesNotThrow()
    {
        await using (var ctx = CreateContext())
        {
            ctx.Transcripts.Add(CreateTranscriptWithCreatedAt(DateTimeOffset.UtcNow.AddDays(-30)));
            await ctx.SaveChangesAsync();
        }

        var act = async () => await CreateSut().RunCleanupAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RunCleanup_LogsDeletionCount()
    {
        var logger = new CapturingLogger<TranscriptRetentionService>();
        await using (var ctx = CreateContext())
        {
            ctx.Transcripts.AddRange(
                CreateTranscriptWithCreatedAt(DateTimeOffset.UtcNow.AddDays(-100)),
                CreateTranscriptWithCreatedAt(DateTimeOffset.UtcNow.AddDays(-100)),
                CreateTranscriptWithCreatedAt(DateTimeOffset.UtcNow.AddDays(-100)));
            await ctx.SaveChangesAsync();
        }

        await CreateSut(logger: logger).RunCleanupAsync(CancellationToken.None);

        logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("Deleted 3 transcript record(s)"));
    }

    [Fact]
    public async Task RunCleanup_OnException_LogsError()
    {
        var logger = new CapturingLogger<TranscriptRetentionService>();
        var sut = CreateSut(logger: logger);
        // Disposing the provider causes scope creation to throw ObjectDisposedException.
        _serviceProvider.Dispose();

        await sut.RunCleanupAsync(CancellationToken.None);

        logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Error &&
            e.Exception != null &&
            e.Message.Contains("Transcript retention cleanup failed"));
    }

    public void Dispose()
    {
        try { _serviceProvider.Dispose(); } catch { /* already disposed in some tests */ }
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception), exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
