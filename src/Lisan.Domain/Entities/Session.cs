using Lisan.Domain.Enums;

namespace Lisan.Domain.Entities;

public class Session
{
    public Guid Id { get; private set; }

    /// <summary>FK to ChildProfile — constraint added when ChildProfile entity is created.</summary>
    public Guid ChildProfileId { get; private set; }

    /// <summary>FK to Persona — constraint added when Persona entity is created.</summary>
    public Guid PersonaId { get; private set; }

    public Guid? StoryId { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    public int? DurationSeconds { get; private set; }

    public int PersianWordsProducedCount { get; private set; }

    public SessionStatus Status { get; private set; }

    /// <summary>Updated on every pipeline activity; drives the abandoned-session detection window.</summary>
    public DateTimeOffset LastActivityAt { get; private set; }

    private Session() { }

    public static Session Start(Guid childProfileId, Guid personaId, Guid? storyId)
    {
        var now = DateTimeOffset.UtcNow;
        return new Session
        {
            Id = Guid.NewGuid(),
            ChildProfileId = childProfileId,
            PersonaId = personaId,
            StoryId = storyId,
            StartedAt = now,
            LastActivityAt = now,
            Status = SessionStatus.Active,
            PersianWordsProducedCount = 0
        };
    }

    public void RecordActivity(DateTimeOffset at)
    {
        if (Status != SessionStatus.Active)
            return;

        LastActivityAt = at;
    }

    public void Complete(DateTimeOffset at)
    {
        if (Status != SessionStatus.Active)
            return;

        Status = SessionStatus.Completed;
        EndedAt = at;
        DurationSeconds = (int)(at - StartedAt).TotalSeconds;
    }

    public void Abandon(DateTimeOffset at)
    {
        if (Status != SessionStatus.Active)
            return;

        Status = SessionStatus.Abandoned;
        EndedAt = at;
        DurationSeconds = (int)(at - StartedAt).TotalSeconds;
    }

    public void AddPersianWords(int count)
    {
        if (count > 0)
            PersianWordsProducedCount += count;
    }
}
