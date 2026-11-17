using Steeltoe.Discovery.Eureka;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEurekaDiscoveryClient();
builder.Services.AddHealthChecks();
builder.Services.AddControllers();

WebApplication app = builder.Build();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => "ServiceC.OpenApi");

await app.RunAsync();
