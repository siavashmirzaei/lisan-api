namespace Lisan.Domain.Entities;

public class Parent
{
    public Guid Id { get; private set; }
    public string ClerkUserId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTimeOffset? ConsentAcceptedAt { get; private set; }
    public string? ConsentVersion { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Parent() { }

    public static Parent Create(string clerkUserId, string email) => new()
    {
        Id = Guid.NewGuid(),
        ClerkUserId = clerkUserId,
        Email = email,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public void RecordConsent(string version, DateTimeOffset at)
    {
        ConsentVersion = version;
        ConsentAcceptedAt = at;
    }
}
