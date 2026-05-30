namespace Lisan.Domain.Entities;

public class ChildProfile
{
    public Guid Id { get; private set; }
    public Guid ParentId { get; private set; }

    /// <summary>First name only — no last name stored (PIPEDA data minimization).</summary>
    public string Name { get; private set; } = string.Empty;
    public int Age { get; private set; }

    /// <summary>FK to Persona — constraint added when Persona entity is created.</summary>
    public Guid? ActivePersonaId { get; private set; }
    public int SpeakingLadderStage { get; private set; }
    public int SessionsCompleted { get; private set; }
    public int? DailyTimeLimitMinutes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private ChildProfile() { }

    public static ChildProfile Create(Guid parentId, string name, int age) => new()
    {
        Id = Guid.NewGuid(),
        ParentId = parentId,
        Name = name,
        Age = age,
        SpeakingLadderStage = 1,
        SessionsCompleted = 0,
        CreatedAt = DateTimeOffset.UtcNow
    };
}
