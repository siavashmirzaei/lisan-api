using Clerk.Net.AspNetCore.Security;
using Lisan.Infrastructure.Extensions;
using Lisan.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Fail fast if any required environment variable is absent.
string[] requiredVars = ["DATABASE_URL", "CLERK_AUTHORITY", "OPENAI_API_KEY", "GOOGLE_TTS_API_KEY", "PRIVACY_POLICY_VERSION"];
var missingVars = requiredVars.Where(v => string.IsNullOrWhiteSpace(builder.Configuration[v])).ToArray();
if (missingVars.Length > 0)
    throw new InvalidOperationException(
        $"Application cannot start. Missing required environment variables: {string.Join(", ", missingVars)}");

var sentryDsn = builder.Configuration["SENTRY_DSN_BACKEND"];
if (!string.IsNullOrEmpty(sentryDsn))
{
    builder.WebHost.UseSentry(o =>
    {
        o.Dsn = sentryDsn;
        o.Environment = builder.Environment.EnvironmentName;
        o.SendDefaultPii = false;
        o.TracesSampleRate = 0;
    });
}

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(ClerkAuthenticationDefaults.AuthenticationScheme)
    .AddClerkAuthentication(x =>
    {
        x.Authority = builder.Configuration["CLERK_AUTHORITY"]!;
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var app = builder.Build();

if (!string.IsNullOrEmpty(sentryDsn))
    app.UseSentryTracing();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", async (AppDbContext db, CancellationToken ct) =>
{
    try
    {
        var connected = await db.Database.CanConnectAsync(ct);
        return connected
            ? Results.Ok(new { db_connected = true })
            : Results.Json(new { db_connected = false }, statusCode: 503);
    }
    catch
    {
        return Results.Json(new { db_connected = false }, statusCode: 503);
    }
}).AllowAnonymous();

app.MapGet("/api/ping", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program { }
