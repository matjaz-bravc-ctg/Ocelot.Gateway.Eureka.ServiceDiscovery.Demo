using Ocelot.DependencyInjection;
using Ocelot.Discovery.Eureka;
using Ocelot.Middleware;
using Ocelot.QualityOfService.Polly;

const string ServiceName = "Services.Gateway";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
  .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
  .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
  // Re-add environment variables so they still take precedence over the Ocelot files.
  .AddEnvironmentVariables();

builder.Logging
  .ClearProviders()
  .AddConsole()
  .AddDebug();

builder.Services
  .AddOcelot(builder.Configuration)
  .AddEureka() // https://ocelot.readthedocs.io/en/latest/features/servicediscovery.html#eureka
  .AddPolly();

WebApplication app = builder.Build();

app.MapGet("/", () => ServiceName);
app.MapGet("/info", (HttpContext context) => $"{ServiceName}, running on {context.Request.Host}");

// Ocelot handles every request that reaches it, so the gateway's own endpoints
// have to be matched and executed before the Ocelot middleware.
app.UseRouting();
app.UseEndpoints(_ => { });
await app.UseOcelot();

await app.RunAsync();
