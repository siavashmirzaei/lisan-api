using Lisan.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisan.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<ChildProfile> ChildProfiles => Set<ChildProfile>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Transcript> Transcripts => Set<Transcript>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
