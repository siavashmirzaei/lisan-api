using Lisan.Application.Repositories;
using Lisan.Domain.Entities;
using Lisan.Infrastructure.Persistence;

namespace Lisan.Infrastructure.Repositories;

public class TranscriptRepository(AppDbContext db) : ITranscriptRepository
{
    public async Task AddAsync(Transcript transcript, CancellationToken cancellationToken = default)
    {
        await db.Transcripts.AddAsync(transcript, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}
