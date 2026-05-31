using Lisan.Application.Repositories;
using Lisan.Infrastructure.Persistence;
using Lisan.Infrastructure.Repositories;
using Lisan.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lisan.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var databaseUrl = configuration["DATABASE_URL"]
                ?? throw new InvalidOperationException("DATABASE_URL environment variable is not set.");
            options.UseNpgsql(databaseUrl);
        });

        services.AddHostedService<AbandonedSessionCleanupService>();
        services.AddHostedService<TranscriptRetentionService>();
        services.AddScoped<ITranscriptRepository, TranscriptRepository>();

        return services;
    }
}
