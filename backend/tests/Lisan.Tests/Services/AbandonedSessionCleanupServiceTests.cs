using FluentAssertions;
using Lisan.Domain.Entities;
using Lisan.Domain.Enums;
using Lisan.Infrastructure.Persistence;
using Lisan.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lisan.Tests.Services;

public class AbandonedSessionCleanupServiceTests : IDisposable
{
    private readonly InMemoryDatabaseRoot _dbRoot = new();
    private const string DbName = "LisanTestDb";
    private readonly ServiceProvider _serviceProvider;
    private readonly AbandonedSessionCleanupService _sut;

    public AbandonedSessionCleanupServiceTests()
    {
        _serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(DbName, _dbRoot))
            .BuildServiceProvider();

        _sut = new AbandonedSessionCleanupService(
            _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<AbandonedSessionCleanupService>.Instance);
    }

    private AppDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(DbName, _dbRoot)
            .Options);

    [Fact]
    public async Task RunCleanup_AbandonsSessions_InactiveForMoreThan10Minutes()
    {
        var session = Session.Start(Guid.NewGuid(), Guid.NewGuid(), storyId: null);
        session.RecordActivity(DateTimeOffset.UtcNow.AddMinutes(-11));
        await using (var ctx = CreateContext()) { ctx.Sessions.Add(session); await ctx.SaveChangesAsync(); }

        await _sut.RunCleanupAsync(CancellationToken.None);

        await using var readCtx = CreateContext();
        var result = await readCtx.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        result!.Status.Should().Be(SessionStatus.Abandoned);
        result.DurationSeconds.Should().NotBeNull();
        result.EndedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RunCleanup_DoesNotAbandonSessions_InactiveForLessThan10Minutes()
    {
        var session = Session.Start(Guid.NewGuid(), Guid.NewGuid(), storyId: null);
        session.RecordActivity(DateTimeOffset.UtcNow.AddMinutes(-9));
        await using (var ctx = CreateContext()) { ctx.Sessions.Add(session); await ctx.SaveChangesAsync(); }

        await _sut.RunCleanupAsync(CancellationToken.None);

        await using var readCtx = CreateContext();
        var result = await readCtx.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        result!.Status.Should().Be(SessionStatus.Active);
        result.EndedAt.Should().BeNull();
    }

    [Fact]
    public async Task RunCleanup_DoesNotTouch_CompletedSessions()
    {
        var session = Session.Start(Guid.NewGuid(), Guid.NewGuid(), storyId: null);
        session.RecordActivity(DateTimeOffset.UtcNow.AddMinutes(-20));
        session.Complete(DateTimeOffset.UtcNow.AddMinutes(-15));
        await using (var ctx = CreateContext()) { ctx.Sessions.Add(session); await ctx.SaveChangesAsync(); }

        await _sut.RunCleanupAsync(CancellationToken.None);

        await using var readCtx = CreateContext();
        var result = await readCtx.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        result!.Status.Should().Be(SessionStatus.Completed);
    }

    [Fact]
    public async Task RunCleanup_DoesNotTouch_AlreadyAbandonedSessions()
    {
        var session = Session.Start(Guid.NewGuid(), Guid.NewGuid(), storyId: null);
        session.RecordActivity(DateTimeOffset.UtcNow.AddMinutes(-20));
        session.Abandon(DateTimeOffset.UtcNow.AddMinutes(-15));
        var originalEndedAt = session.EndedAt;
        await using (var ctx = CreateContext()) { ctx.Sessions.Add(session); await ctx.SaveChangesAsync(); }

        await _sut.RunCleanupAsync(CancellationToken.None);

        await using var readCtx = CreateContext();
        var result = await readCtx.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        result!.Status.Should().Be(SessionStatus.Abandoned);
        result.EndedAt.Should().Be(originalEndedAt);
    }

    [Fact]
    public async Task RunCleanup_SetsDurationSeconds_WhenAbandoning()
    {
        var session = Session.Start(Guid.NewGuid(), Guid.NewGuid(), storyId: null);
        session.RecordActivity(DateTimeOffset.UtcNow.AddMinutes(-11));
        await using (var ctx = CreateContext()) { ctx.Sessions.Add(session); await ctx.SaveChangesAsync(); }

        await _sut.RunCleanupAsync(CancellationToken.None);

        await using var readCtx = CreateContext();
        var result = await readCtx.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        result!.DurationSeconds.Should().NotBeNull();
        result.DurationSeconds.Should().BeGreaterThanOrEqualTo(0);
    }

    public void Dispose() => _serviceProvider.Dispose();
}
