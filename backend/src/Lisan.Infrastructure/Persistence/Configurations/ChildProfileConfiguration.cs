using Lisan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisan.Infrastructure.Persistence.Configurations;

public class ChildProfileConfiguration : IEntityTypeConfiguration<ChildProfile>
{
    public void Configure(EntityTypeBuilder<ChildProfile> builder)
    {
        builder.ToTable("child_profiles");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.ParentId)
            .IsRequired()
            .HasColumnName("parent_id");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasColumnName("name");

        builder.Property(c => c.Age)
            .IsRequired()
            .HasColumnName("age");

        builder.Property(c => c.ActivePersonaId)
            .HasColumnName("active_persona_id");

        builder.Property(c => c.SpeakingLadderStage)
            .IsRequired()
            .HasDefaultValue(1)
            .HasColumnName("speaking_ladder_stage");

        builder.Property(c => c.SessionsCompleted)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("sessions_completed");

        builder.Property(c => c.DailyTimeLimitMinutes)
            .HasColumnName("daily_time_limit_minutes");

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.HasOne<Parent>()
            .WithMany()
            .HasForeignKey(c => c.ParentId)
            .HasConstraintName("FK_child_profiles_parents_parent_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ParentId).HasDatabaseName("ix_child_profiles_parent_id");
    }
}
