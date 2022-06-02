using System.Diagnostics;
using Honeycomb.OpenTelemetry;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var appBuilder = WebApplication.CreateBuilder(args);

appBuilder.Services.AddOpenTelemetryTracing(builder => 
    builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("OpenTelemetry Demo (Product API)"))
           .AddAspNetCoreInstrumentation()
           .AddJaegerExporter()
           .AddHoneycomb(o => {
               o.ApiKey = appBuilder.Configuration["HoneycombSettings:ApiKey"];
               o.ServiceName = "Product Service";
           }));

var app = appBuilder.Build();

app.MapGet("/productinfo/{id}", async (int id, HttpContext ctx) => {
    await Task.Delay(500);

    var accountId = Baggage.GetBaggage("account_id");
    Activity.Current?.SetTag("account_id", accountId);
    Activity.Current?.SetTag("product_id", id);

    await ctx.Response.WriteAsJsonAsync(new {
        productId = id,
        productName = "Widgets"
    });
});

app.Run();
