using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ServiceC.OpenApi.Tests;

/// <summary>
/// Hosts the app in memory with the Eureka client switched off,
/// so the tests run without a Eureka server.
/// </summary>
public sealed class ServiceCFactory : WebApplicationFactory<Program>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder) =>
    builder.ConfigureAppConfiguration((_, config) =>
      config.AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["Eureka:Client:Enabled"] = "false"
      }));
}
