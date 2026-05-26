using FluentAssertions;
using Lisan.Domain.Entities;
using Lisan.Domain.Enums;
using Lisan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lisan.Tests.Persistence;

public class TranscriptCascadeDeleteTests : IDisposable
{
    private readonly InMemoryDatabaseRoot _dbRoot = new();
    private const string DbName = "LisanTranscriptCascadeTestDb";

    private AppDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(DbName, _dbRoot)
            .Options);

    [Fact]
    public async Task DeletingSession_CascadesAndDeletesItsTranscripts()
    {
        var session = Session.Start(Guid.NewGuid(), Guid.NewGuid(), storyId: null);
        var transcript1 = Transcript.Record(session.Id, turn: 1, TranscriptSpeaker.Child, "سلام");
        var transcript2 = Transcript.Record(session.Id, turn: 2, TranscriptSpeaker.Companion, "خوبی؟");
        var otherSession = Session.Start(Guid.NewGuid(), Guid.NewGuid(), storyId: null);
        var otherTranscript = Transcript.Record(otherSession.Id, turn: 1, TranscriptSpeaker.Child, "ممنون");

        await using (var ctx = CreateContext())
        {
            ctx.Sessions.AddRange(session, otherSession);
            ctx.Transcripts.AddRange(transcript1, transcript2, otherTranscript);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var s = await ctx.Sessions.FindAsync(session.Id);
            await ctx.Transcripts.Where(t => t.SessionId == session.Id).LoadAsync();
            ctx.Sessions.Remove(s!);
            await ctx.SaveChangesAsync();
        }

        await using var readCtx = CreateContext();
        var remaining = await readCtx.Transcripts.ToListAsync();
        remaining.Should().NotContain(t => t.SessionId == session.Id);
        remaining.Should().ContainSingle(t => t.SessionId == otherSession.Id);
    }

    [Fact]
    public async Task DeletingSession_DoesNotAffectTranscriptsOfOtherSessions()
    {
        var sessionA = Session.Start(Guid.NewGuid(), Guid.NewGuid(), storyId: null);
        var sessionB = Session.Start(Guid.NewGuid(), Guid.NewGuid(), storyId: null);
        var transcriptA = Transcript.Record(sessionA.Id, turn: 1, TranscriptSpeaker.Child, "یک");
        var transcriptB = Transcript.Record(sessionB.Id, turn: 1, TranscriptSpeaker.Child, "دو");

        await using (var ctx = CreateContext())
        {
            ctx.Sessions.AddRange(sessionA, sessionB);
            ctx.Transcripts.AddRange(transcriptA, transcriptB);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var s = await ctx.Sessions.FindAsync(sessionA.Id);
            await ctx.Transcripts.Where(t => t.SessionId == sessionA.Id).LoadAsync();
            ctx.Sessions.Remove(s!);
            await ctx.SaveChangesAsync();
        }

        await using var readCtx = CreateContext();
        var remaining = await readCtx.Transcripts.SingleAsync();
        remaining.Id.Should().Be(transcriptB.Id);
    }

    public void Dispose() { }
}
