
using System.Diagnostics;
using Honeycomb.OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var appBuilder = WebApplication.CreateBuilder(args);

var source = new ActivitySource("Delay Test");

appBuilder.Services.AddOpenTelemetryTracing(builder => {
    builder
        .AddAspNetCoreInstrumentation()
        .AddSource("Delay Test")
        .AddHoneycomb(o => {
            o.ApiKey = "";
        })
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("OpenTelemetry Demo"))
        .AddConsoleExporter();
});

var app = appBuilder.Build();

app.MapGet("/", async () => {

    await Task.Delay(100);
    using (var span = source.StartActivity("Delayed Process"))
    {
        span?.AddTag("Type", "Awesome Thing");
        await Task.Delay(200);
    }

    return "Hello World";
});

app.Run();
