using Lisan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisan.Infrastructure.Persistence.Configurations;

public class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.ToTable("parents");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.ClerkUserId)
            .IsRequired()
            .HasColumnName("clerk_user_id");

        builder.Property(p => p.Email)
            .IsRequired()
            .HasColumnName("email");

        builder.Property(p => p.ConsentAcceptedAt)
            .HasColumnName("consent_accepted_at");

        builder.Property(p => p.ConsentVersion)
            .HasColumnName("consent_version");

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.HasIndex(p => p.ClerkUserId)
            .IsUnique()
            .HasDatabaseName("ix_parents_clerk_user_id");
    }
}
