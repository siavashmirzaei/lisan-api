using Lisan.Infrastructure.Extensions;
using Lisan.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry(o =>
{
    o.Dsn = builder.Configuration["SENTRY_DSN_BACKEND"];
    o.Environment = builder.Environment.EnvironmentName;
    o.SendDefaultPii = false;
    o.TracesSampleRate = 0;
});

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSentryTracing();

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
});

app.Run();
