using Lisan.Domain.Entities;

namespace Lisan.Application.Repositories;

public interface ITranscriptRepository
{
    Task AddAsync(Transcript transcript, CancellationToken cancellationToken = default);
}
