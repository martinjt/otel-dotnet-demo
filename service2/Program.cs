using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetryTracing(builder => 
    builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("OpenTelemetry Demo (Product API)"))
           .AddSource("OpenTelemetry.Demo.ProductAPI")
           .AddAspNetCoreInstrumentation()
           .AddJaegerExporter());


var app = builder.Build();


ActivitySource source = new ActivitySource("OpenTelemetry.Demo.ProductAPI");

app.MapGet("/", async (ctx) => {
    await Task.Delay(500);

    var accountId = Baggage.GetBaggage("AccountId");
    Activity.Current?.SetTag("AccountId", accountId);

    await ctx.Response.WriteAsJsonAsync(new {
        productId = "123",
        productName = "Widgets"
    });
});

app.Run();
