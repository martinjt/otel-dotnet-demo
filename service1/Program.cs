using System.Diagnostics;
using Honeycomb.OpenTelemetry;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

ActivitySource source = new ActivitySource("Frontend Service");

var appBuilder = WebApplication.CreateBuilder(args);

appBuilder.Services.AddOpenTelemetryTracing(builder => 
    builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("OpenTelemetry Demo - Frontend Service"))
           .AddSource(source.Name)
           .AddAspNetCoreInstrumentation()
           .AddJaegerExporter()
           .AddHoneycomb(o => {
               o.ApiKey = appBuilder.Configuration["HoneycombSettings:ApiKey"];
               o.ServiceName = "Frontend Service";
           }));

appBuilder.Services.AddHttpClient();

var app = appBuilder.Build();


app.MapGet("/", async () => {
    await Task.Delay(500);
    return "Hello world";
});

app.MapGet("/child", async () => {
    var accountId = Guid.NewGuid();
    await Task.Delay(500);

    using (var activity = source.StartActivity("Get Account Info"))
    {
        Baggage.SetBaggage("account_id", accountId.ToString());
        activity?.SetTag("account_type", "VIP");
        await Task.Delay(500);
        return "Hello world";
    }
});


app.MapGet("/baggage", async (ctx) => {
    var httpclient = ctx.RequestServices.GetRequiredService<HttpClient>();
    httpclient.BaseAddress = new Uri("http://localhost:5041");

    var accountId = Guid.NewGuid();
    await Task.Delay(500);

    using (var accountActivity = source.StartActivity("Get Account Info"))
    {
        Baggage.SetBaggage("account_id", accountId.ToString());
        accountActivity?.SetTag("account_type", "VIP");
        
        await Task.Delay(500);
    }

    using (var productActivity = source.StartActivity("Get Product Info"))
    {
        productActivity?.SetTag("product_id", "1234");

        var resp = await httpclient.GetAsync("/productinfo/1234");
        var productInfo = await resp.Content.ReadFromJsonAsync<ProductInfo>();

        await ctx.Response.WriteAsJsonAsync(new {
            AccountId = accountId,
            ProductName = productInfo?.ProductName
        });
    }
});

app.Run();

record ProductInfo(int ProductId, string ProductName);
