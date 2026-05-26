using Lisan.Infrastructure.Extensions;

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

app.UseHttpsRedirection();
app.UseSentryTracing();

app.Run();
