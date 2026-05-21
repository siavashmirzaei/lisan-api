using Lisan.Domain.Entities;
using Lisan.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisan.Infrastructure.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.ChildProfileId)
            .IsRequired()
            .HasColumnName("child_profile_id");

        builder.Property(s => s.PersonaId)
            .IsRequired()
            .HasColumnName("persona_id");

        builder.Property(s => s.StoryId)
            .HasColumnName("story_id");

        builder.Property(s => s.StartedAt)
            .IsRequired()
            .HasColumnName("started_at");

        builder.Property(s => s.EndedAt)
            .HasColumnName("ended_at");

        builder.Property(s => s.DurationSeconds)
            .HasColumnName("duration_seconds");

        builder.Property(s => s.PersianWordsProducedCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("persian_words_produced_count");

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnName("status");

        builder.Property(s => s.LastActivityAt)
            .IsRequired()
            .HasColumnName("last_activity_at");

        builder.HasIndex(s => s.ChildProfileId).HasDatabaseName("ix_sessions_child_profile_id");
        builder.HasIndex(s => s.Status).HasDatabaseName("ix_sessions_status");
    }
}
