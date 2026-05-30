using FluentAssertions;
using Lisan.Domain.Entities;

namespace Lisan.Tests.Domain;

public class ChildProfileTests
{
    private static readonly Guid ParentId = Guid.NewGuid();

    [Fact]
    public void Create_InitializesRequiredFields()
    {
        var before = DateTimeOffset.UtcNow;

        var profile = ChildProfile.Create(ParentId, "Ali", 5);

        profile.Id.Should().NotBeEmpty();
        profile.ParentId.Should().Be(ParentId);
        profile.Name.Should().Be("Ali");
        profile.Age.Should().Be(5);
        profile.CreatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Create_SetsDefaultProgressFields()
    {
        var profile = ChildProfile.Create(ParentId, "Ali", 5);

        profile.SpeakingLadderStage.Should().Be(1);
        profile.SessionsCompleted.Should().Be(0);
        profile.ActivePersonaId.Should().BeNull();
        profile.DailyTimeLimitMinutes.Should().BeNull();
    }

    [Fact]
    public void Create_AssignsUniqueIds()
    {
        var a = ChildProfile.Create(ParentId, "Ali", 5);
        var b = ChildProfile.Create(ParentId, "Sara", 7);

        a.Id.Should().NotBe(b.Id);
    }
}
