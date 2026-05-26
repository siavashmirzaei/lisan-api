using Lisan.Domain.Enums;

namespace Lisan.Domain.Entities;

public class Transcript
{
    public Guid Id { get; private set; }

    /// <summary>FK to Session — cascade deletes this record when the session is deleted.</summary>
    public Guid SessionId { get; private set; }

    public int Turn { get; private set; }

    public TranscriptSpeaker Speaker { get; private set; }

    /// <summary>Persian text produced in this turn. Text only — no audio data ever stored.</summary>
    public string TextFa { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    private Transcript() { }

    public static Transcript Record(Guid sessionId, int turn, TranscriptSpeaker speaker, string textFa)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId must not be empty.", nameof(sessionId));

        if (turn <= 0)
            throw new ArgumentOutOfRangeException(nameof(turn), "Turn must be a positive integer.");

        if (string.IsNullOrWhiteSpace(textFa))
            throw new ArgumentException("Transcript text must not be empty.", nameof(textFa));

        return new Transcript
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Turn = turn,
            Speaker = speaker,
            TextFa = textFa,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
