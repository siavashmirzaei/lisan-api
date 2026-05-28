using Clerk.Net.AspNetCore.Security;
using Lisan.Infrastructure.Extensions;
using Lisan.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

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
        x.Authority = builder.Configuration["CLERK_AUTHORITY"]
            ?? throw new InvalidOperationException("CLERK_AUTHORITY is not configured.");
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
