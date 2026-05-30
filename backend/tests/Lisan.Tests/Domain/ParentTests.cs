using FluentAssertions;
using Lisan.Domain.Entities;

namespace Lisan.Tests.Domain;

public class ParentTests
{
    [Fact]
    public void Create_InitializesRequiredFields()
    {
        var before = DateTimeOffset.UtcNow;

        var parent = Parent.Create("user_abc123", "parent@example.com");

        parent.Id.Should().NotBeEmpty();
        parent.ClerkUserId.Should().Be("user_abc123");
        parent.Email.Should().Be("parent@example.com");
        parent.CreatedAt.Should().BeOnOrAfter(before);
        parent.ConsentAcceptedAt.Should().BeNull();
        parent.ConsentVersion.Should().BeNull();
    }

    [Fact]
    public void Create_AssignsUniqueIds()
    {
        var a = Parent.Create("user_a", "a@example.com");
        var b = Parent.Create("user_b", "b@example.com");

        a.Id.Should().NotBe(b.Id);
    }

    [Fact]
    public void RecordConsent_SetsConsentFields()
    {
        var parent = Parent.Create("user_abc123", "parent@example.com");
        var at = DateTimeOffset.UtcNow;

        parent.RecordConsent("v1.0", at);

        parent.ConsentVersion.Should().Be("v1.0");
        parent.ConsentAcceptedAt.Should().Be(at);
    }

    [Fact]
    public void RecordConsent_CanBeUpdated()
    {
        var parent = Parent.Create("user_abc123", "parent@example.com");
        parent.RecordConsent("v1.0", DateTimeOffset.UtcNow.AddDays(-30));
        var renewedAt = DateTimeOffset.UtcNow;

        parent.RecordConsent("v2.0", renewedAt);

        parent.ConsentVersion.Should().Be("v2.0");
        parent.ConsentAcceptedAt.Should().Be(renewedAt);
    }
}
