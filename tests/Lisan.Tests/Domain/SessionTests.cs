using FluentAssertions;
using Lisan.Domain.Entities;
using Lisan.Domain.Enums;

namespace Lisan.Tests.Domain;

public class SessionTests
{
    private static readonly Guid ChildProfileId = Guid.NewGuid();
    private static readonly Guid PersonaId = Guid.NewGuid();

    [Fact]
    public void Start_CreatesActiveSession()
    {
        var session = Session.Start(ChildProfileId, PersonaId, storyId: null);

        session.Status.Should().Be(SessionStatus.Active);
        session.ChildProfileId.Should().Be(ChildProfileId);
        session.PersonaId.Should().Be(PersonaId);
        session.PersianWordsProducedCount.Should().Be(0);
        session.EndedAt.Should().BeNull();
        session.DurationSeconds.Should().BeNull();
    }

    [Fact]
    public void Complete_SetsStatusAndDuration()
    {
        var session = Session.Start(ChildProfileId, PersonaId, storyId: null);
        var endTime = session.StartedAt.AddMinutes(7);

        session.Complete(endTime);

        session.Status.Should().Be(SessionStatus.Completed);
        session.EndedAt.Should().Be(endTime);
        session.DurationSeconds.Should().Be(420);
    }

    [Fact]
    public void Abandon_SetsStatusAndDuration()
    {
        var session = Session.Start(ChildProfileId, PersonaId, storyId: null);
        var abandonTime = session.StartedAt.AddMinutes(12);

        session.Abandon(abandonTime);

        session.Status.Should().Be(SessionStatus.Abandoned);
        session.EndedAt.Should().Be(abandonTime);
        session.DurationSeconds.Should().Be(720);
    }

    [Fact]
    public void Abandon_WhenAlreadyCompleted_DoesNothing()
    {
        var session = Session.Start(ChildProfileId, PersonaId, storyId: null);
        var endTime = session.StartedAt.AddMinutes(5);
        session.Complete(endTime);

        session.Abandon(endTime.AddMinutes(2));

        session.Status.Should().Be(SessionStatus.Completed);
        session.DurationSeconds.Should().Be(300);
    }

    [Fact]
    public void RecordActivity_UpdatesLastActivityAt()
    {
        var session = Session.Start(ChildProfileId, PersonaId, storyId: null);
        var activityTime = session.StartedAt.AddMinutes(3);

        session.RecordActivity(activityTime);

        session.LastActivityAt.Should().Be(activityTime);
    }

    [Fact]
    public void RecordActivity_WhenAbandoned_DoesNothing()
    {
        var session = Session.Start(ChildProfileId, PersonaId, storyId: null);
        var originalActivity = session.LastActivityAt;
        session.Abandon(session.StartedAt.AddMinutes(11));

        session.RecordActivity(session.StartedAt.AddMinutes(12));

        session.LastActivityAt.Should().Be(originalActivity);
    }

    [Fact]
    public void AddPersianWords_AccumulatesCount()
    {
        var session = Session.Start(ChildProfileId, PersonaId, storyId: null);

        session.AddPersianWords(3);
        session.AddPersianWords(2);

        session.PersianWordsProducedCount.Should().Be(5);
    }

    [Fact]
    public void AddPersianWords_IgnoresNonPositiveValues()
    {
        var session = Session.Start(ChildProfileId, PersonaId, storyId: null);

        session.AddPersianWords(0);
        session.AddPersianWords(-1);

        session.PersianWordsProducedCount.Should().Be(0);
    }
}
