using Lisan.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lisan.Tests.Auth;

public class LisanApiFactory : WebApplicationFactory<Program>
{
    public LisanApiFactory()
    {
        // Must be set before EnsureServer() runs Program.cs, because service
        // registrations in Program.cs read these at startup.
        Environment.SetEnvironmentVariable("DATABASE_URL",
            "postgresql://postgres:test@localhost:5432/testdb");
        Environment.SetEnvironmentVariable("CLERK_AUTHORITY",
            "https://test.clerk.accounts.dev");
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", "sk-test-placeholder");
        Environment.SetEnvironmentVariable("GOOGLE_TTS_API_KEY", "test-tts-key");
        Environment.SetEnvironmentVariable("PRIVACY_POLICY_VERSION", "v1.0");
        // Empty string disables Sentry (null causes ArgumentNullException).
        Environment.SetEnvironmentVariable("SENTRY_DSN_BACKEND", "");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace real DB with in-memory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);
            services.AddDbContext<AppDbContext>(
                o => o.UseInMemoryDatabase("LisanAuthTest"));

            // Override auth: swap Clerk JWT Bearer for a test-friendly scheme
            services.PostConfigureAll<AuthenticationOptions>(o =>
            {
                o.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                o.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                o.DefaultScheme = TestAuthHandler.SchemeName;
            });
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            Environment.SetEnvironmentVariable("DATABASE_URL", null);
            Environment.SetEnvironmentVariable("CLERK_AUTHORITY", null);
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", null);
            Environment.SetEnvironmentVariable("GOOGLE_TTS_API_KEY", null);
            Environment.SetEnvironmentVariable("PRIVACY_POLICY_VERSION", null);
            Environment.SetEnvironmentVariable("SENTRY_DSN_BACKEND", null);
        }
    }
}
