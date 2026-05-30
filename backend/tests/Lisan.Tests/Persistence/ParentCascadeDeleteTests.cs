using FluentAssertions;
using Lisan.Domain.Entities;
using Lisan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lisan.Tests.Persistence;

public class ParentCascadeDeleteTests : IDisposable
{
    private readonly InMemoryDatabaseRoot _dbRoot = new();
    private const string DbName = "LisanParentCascadeTestDb";

    private AppDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(DbName, _dbRoot)
            .Options);

    [Fact]
    public async Task DeletingParent_CascadesAndRemovesAllChildProfiles()
    {
        var parent = Parent.Create("user_abc123", "parent@example.com");
        var child1 = ChildProfile.Create(parent.Id, "Ali", 5);
        var child2 = ChildProfile.Create(parent.Id, "Sara", 7);
        var otherParent = Parent.Create("user_other456", "other@example.com");
        var otherChild = ChildProfile.Create(otherParent.Id, "Dara", 6);

        await using (var ctx = CreateContext())
        {
            ctx.Parents.AddRange(parent, otherParent);
            ctx.ChildProfiles.AddRange(child1, child2, otherChild);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var p = await ctx.Parents.FindAsync(parent.Id);
            await ctx.ChildProfiles.Where(c => c.ParentId == parent.Id).LoadAsync();
            ctx.Parents.Remove(p!);
            await ctx.SaveChangesAsync();
        }

        await using var readCtx = CreateContext();
        var remaining = await readCtx.ChildProfiles.ToListAsync();
        remaining.Should().NotContain(c => c.ParentId == parent.Id);
        remaining.Should().ContainSingle(c => c.ParentId == otherParent.Id);
    }

    [Fact]
    public async Task DeletingParent_DoesNotAffectChildProfilesOfOtherParents()
    {
        var parentA = Parent.Create("user_aaa", "a@example.com");
        var parentB = Parent.Create("user_bbb", "b@example.com");
        var childA = ChildProfile.Create(parentA.Id, "Ali", 5);
        var childB = ChildProfile.Create(parentB.Id, "Sara", 7);

        await using (var ctx = CreateContext())
        {
            ctx.Parents.AddRange(parentA, parentB);
            ctx.ChildProfiles.AddRange(childA, childB);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var p = await ctx.Parents.FindAsync(parentA.Id);
            await ctx.ChildProfiles.Where(c => c.ParentId == parentA.Id).LoadAsync();
            ctx.Parents.Remove(p!);
            await ctx.SaveChangesAsync();
        }

        await using var readCtx = CreateContext();
        var remaining = await readCtx.ChildProfiles.SingleAsync();
        remaining.Id.Should().Be(childB.Id);
    }

    public void Dispose() { }
}
