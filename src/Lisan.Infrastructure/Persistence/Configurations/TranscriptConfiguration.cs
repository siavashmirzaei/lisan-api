using Lisan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisan.Infrastructure.Persistence.Configurations;

public class TranscriptConfiguration : IEntityTypeConfiguration<Transcript>
{
    public void Configure(EntityTypeBuilder<Transcript> builder)
    {
        builder.ToTable("transcripts");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever().HasColumnName("id");

        builder.Property(t => t.SessionId)
            .IsRequired()
            .HasColumnName("session_id");

        builder.Property(t => t.Turn)
            .IsRequired()
            .HasColumnName("turn");

        builder.Property(t => t.Speaker)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnName("speaker");

        builder.Property(t => t.TextFa)
            .IsRequired()
            .HasColumnName("text_fa");

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.HasOne<Session>()
            .WithMany()
            .HasForeignKey(t => t.SessionId)
            .HasConstraintName("FK_transcripts_sessions_session_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.SessionId).HasDatabaseName("ix_transcripts_session_id");
    }
}
