using System.Net;

namespace ServiceC.OpenApi.Tests;

public class ServiceEndpointTests(ServiceCFactory factory) : IClassFixture<ServiceCFactory>
{
  private const string ServiceName = "ServiceC.OpenApi";

  private readonly HttpClient _client = factory.CreateClient();

  private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

  [Fact]
  public async Task Root_ReturnsServiceName()
  {
    string body = await _client.GetStringAsync("/", CancellationToken);

    Assert.Equal(ServiceName, body);
  }

  [Fact]
  public async Task Values_ReturnsServiceNameAndRequestDetails()
  {
    string body = await _client.GetStringAsync("/api/values", CancellationToken);

    Assert.Equal($"{ServiceName}, Url: http://localhost, Method: GET, Path: /api/values -> Value", body);
  }

  [Fact]
  public async Task Status_ReturnsHost()
  {
    string body = await _client.GetStringAsync("/api/values/status", CancellationToken);

    Assert.Equal("Running on localhost", body);
  }

  [Fact]
  public async Task Health_ReturnsHealthy()
  {
    HttpResponseMessage response = await _client.GetAsync("/health", CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(CancellationToken));
  }

  [Fact]
  public async Task OldHealthcheckEndpoint_IsGone()
  {
    HttpResponseMessage response = await _client.GetAsync("/api/values/healthcheck", CancellationToken);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task BadCode_ReturnsServerError()
  {
    HttpResponseMessage response = await _client.GetAsync("/api/values/badcode", CancellationToken);

    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
  }
}
