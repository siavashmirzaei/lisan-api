using FluentAssertions;
using Lisan.Domain.Entities;
using Lisan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lisan.Tests.Persistence;

public class ChildProfileCascadeDeleteTests : IDisposable
{
    private readonly InMemoryDatabaseRoot _dbRoot = new();
    private const string DbName = "LisanCascadeTestDb";

    private AppDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(DbName, _dbRoot)
            .Options);

    [Fact]
    public async Task DeletingChildProfile_CascadesAndRemovesAllAssociatedSessions()
    {
        var childProfile = ChildProfile.Create();
        var session1 = Session.Start(childProfile.Id, Guid.NewGuid(), storyId: null);
        var session2 = Session.Start(childProfile.Id, Guid.NewGuid(), storyId: null);
        var otherProfile = ChildProfile.Create();
        var otherSession = Session.Start(otherProfile.Id, Guid.NewGuid(), storyId: null);

        await using (var ctx = CreateContext())
        {
            ctx.ChildProfiles.AddRange(childProfile, otherProfile);
            ctx.Sessions.AddRange(session1, session2, otherSession);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var profile = await ctx.ChildProfiles.FindAsync(childProfile.Id);
            await ctx.Sessions.Where(s => s.ChildProfileId == childProfile.Id).LoadAsync();
            ctx.ChildProfiles.Remove(profile!);
            await ctx.SaveChangesAsync();
        }

        await using var readCtx = CreateContext();
        var remainingSessions = await readCtx.Sessions.ToListAsync();
        remainingSessions.Should().NotContain(s => s.ChildProfileId == childProfile.Id);
        remainingSessions.Should().ContainSingle(s => s.ChildProfileId == otherProfile.Id);
    }

    [Fact]
    public async Task DeletingChildProfile_DoesNotAffectSessionsOfOtherProfiles()
    {
        var profileA = ChildProfile.Create();
        var profileB = ChildProfile.Create();
        var sessionA = Session.Start(profileA.Id, Guid.NewGuid(), storyId: null);
        var sessionB = Session.Start(profileB.Id, Guid.NewGuid(), storyId: null);

        await using (var ctx = CreateContext())
        {
            ctx.ChildProfiles.AddRange(profileA, profileB);
            ctx.Sessions.AddRange(sessionA, sessionB);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var profile = await ctx.ChildProfiles.FindAsync(profileA.Id);
            await ctx.Sessions.Where(s => s.ChildProfileId == profileA.Id).LoadAsync();
            ctx.ChildProfiles.Remove(profile!);
            await ctx.SaveChangesAsync();
        }

        await using var readCtx = CreateContext();
        var remaining = await readCtx.Sessions.SingleAsync();
        remaining.Id.Should().Be(sessionB.Id);
    }

    public void Dispose() { }
}
