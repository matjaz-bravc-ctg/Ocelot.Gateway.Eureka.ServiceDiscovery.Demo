using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Services.Gateway.Tests;

public class GatewayTests(GatewayFactory factory) : IClassFixture<GatewayFactory>
{
  private readonly HttpClient _client = factory.CreateClient();

  private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

  [Fact]
  public async Task Root_ReturnsGatewayName()
  {
    string body = await _client.GetStringAsync("/", CancellationToken);

    Assert.Equal("Services.Gateway", body);
  }

  [Fact]
  public async Task Info_ReturnsGatewayNameAndHost()
  {
    string body = await _client.GetStringAsync("/info", CancellationToken);

    Assert.Equal("Services.Gateway, running on localhost", body);
  }

  [Fact]
  public async Task ApiRoute_IsHandledByOcelotThroughEureka_WhenNoServiceIsRegistered()
  {
    // The request falls through the gateway's own endpoints to Ocelot, which matches the
    // SERVICE.OPENAPI route and asks Eureka for instances. With no Eureka server there is
    // no ServiceA/B/C instance to route to, so Ocelot answers 404.
    HttpResponseMessage response = await _client.GetAsync("/api/values", CancellationToken);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    Assert.Contains(
      factory.LogMessages,
      message => message.StartsWith("Ocelot.ServiceDiscovery.ServiceDiscoveryProviderFactory:")
        && message.Contains("Getting service discovery provider of Type 'Eureka'"));
    Assert.Contains(
      factory.LogMessages,
      message => message.StartsWith("Ocelot.Responder.Middleware.ResponderMiddleware:"));
  }

  [Fact]
  public void OcelotRoute_UsesEurekaServiceWithRoundRobinAndQoS()
  {
    IConfiguration configuration = factory.Services.GetRequiredService<IConfiguration>();
    IConfigurationSection route = configuration.GetSection("Routes:0");

    Assert.Equal("SERVICE.OPENAPI", route["ServiceName"]);
    Assert.Equal("RoundRobin", route["LoadBalancerOptions:Type"]);
    Assert.Equal("3", route["QoSOptions:ExceptionsAllowedBeforeBreaking"]);
    Assert.Equal("Eureka", configuration["GlobalConfiguration:ServiceDiscoveryProvider:Type"]);
  }
}
