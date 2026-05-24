using FluentAssertions;
using Lisan.Domain.Entities;
using Lisan.Domain.Enums;

namespace Lisan.Tests.Domain;

public class TranscriptTests
{
    private static readonly Guid SessionId = Guid.NewGuid();

    [Fact]
    public void Record_CreatesTranscriptWithCorrectFields()
    {
        var before = DateTimeOffset.UtcNow;

        var transcript = Transcript.Record(SessionId, turn: 1, TranscriptSpeaker.Child, "سلام");

        transcript.Id.Should().NotBeEmpty();
        transcript.SessionId.Should().Be(SessionId);
        transcript.Turn.Should().Be(1);
        transcript.Speaker.Should().Be(TranscriptSpeaker.Child);
        transcript.TextFa.Should().Be("سلام");
        transcript.CreatedAt.Should().BeOnOrAfter(before);
        transcript.CreatedAt.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Record_CompanionSpeaker_IsStoredCorrectly()
    {
        var transcript = Transcript.Record(SessionId, turn: 2, TranscriptSpeaker.Companion, "چطوری؟");

        transcript.Speaker.Should().Be(TranscriptSpeaker.Companion);
        transcript.Turn.Should().Be(2);
    }

    [Fact]
    public void Record_WithEmptyText_ThrowsArgumentException()
    {
        var act = () => Transcript.Record(SessionId, turn: 1, TranscriptSpeaker.Child, "");

        act.Should().Throw<ArgumentException>().WithParameterName("textFa");
    }

    [Fact]
    public void Record_WithWhitespaceText_ThrowsArgumentException()
    {
        var act = () => Transcript.Record(SessionId, turn: 1, TranscriptSpeaker.Child, "   ");

        act.Should().Throw<ArgumentException>().WithParameterName("textFa");
    }

    [Fact]
    public void Record_EachCallProducesUniqueId()
    {
        var t1 = Transcript.Record(SessionId, turn: 1, TranscriptSpeaker.Child, "سلام");
        var t2 = Transcript.Record(SessionId, turn: 2, TranscriptSpeaker.Companion, "خوبی؟");

        t1.Id.Should().NotBe(t2.Id);
    }

    [Fact]
    public void Transcript_HasNoAudioFields()
    {
        var properties = typeof(Transcript).GetProperties();

        properties.Should().NotContain(p =>
            p.Name.Contains("Audio", StringComparison.OrdinalIgnoreCase) ||
            p.Name.Contains("File", StringComparison.OrdinalIgnoreCase) ||
            p.Name.Contains("Path", StringComparison.OrdinalIgnoreCase) ||
            p.Name.Contains("Url", StringComparison.OrdinalIgnoreCase));
    }
}
