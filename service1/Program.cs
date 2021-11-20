using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetryTracing(builder => 
    builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("OpenTelemetry Demo"))
           .AddSource("OpenTelemetry.Demo")
           .AddAspNetCoreInstrumentation()
           .AddHttpClientInstrumentation()
           .AddJaegerExporter());

builder.Services.AddHttpClient();


var app = builder.Build();


ActivitySource source = new ActivitySource("OpenTelemetry.Demo");

app.MapGet("/", async () => {
    await Task.Delay(500);
    return "Hello world";
});

app.MapGet("/child", async () => {
    var accountId = Guid.NewGuid();
    await Task.Delay(500);

    using (var activity = source.StartActivity("Get Account Info"))
    {
        Baggage.SetBaggage("AccountId", accountId.ToString());
        activity?.SetTag("AccountType", "VIP");
        await Task.Delay(500);
        return "Hello world";
    }
});


app.MapGet("/baggage", async (ctx) => {
    var httpclient = ctx.RequestServices.GetRequiredService<HttpClient>();

    var accountId = Guid.NewGuid();
    await Task.Delay(500);

    using (var activity = source.StartActivity("Get Account Info"))
    {
        Baggage.SetBaggage("AccountId", accountId.ToString());
        activity?.SetTag("AccountType", "VIP");
        await Task.Delay(500);

        using (var childActivity = source.StartActivity("Get Product Info"))
        {
            activity?.SetTag("ProductId", "1234");

            var resp = await httpclient.GetAsync("/productinfo/1234");
            var productInfo = await resp.Content.ReadFromJsonAsync<ProductInfo>();

            await ctx.Response.WriteAsJsonAsync(new {
                AccountId = accountId,
                ProductName = productInfo?.ProductName
            });
        }
    }
});

app.Run();

record ProductInfo(string ProductId, string ProductName);
