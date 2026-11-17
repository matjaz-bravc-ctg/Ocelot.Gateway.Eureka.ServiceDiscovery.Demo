using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Services.Gateway.Tests;

/// <summary>
/// Hosts the gateway in memory with the Eureka client switched off, so the tests run
/// without a Eureka server, and records its log messages so tests can see what Ocelot did.
/// </summary>
public sealed class GatewayFactory : WebApplicationFactory<Program>
{
  private readonly CapturingLoggerProvider _logs = new();

  /// <summary>
  /// Log messages written by the gateway, as "category: message".
  /// </summary>
  public IReadOnlyCollection<string> LogMessages => _logs.Messages;

  protected override void ConfigureWebHost(IWebHostBuilder builder) =>
    builder
      .ConfigureAppConfiguration((_, config) =>
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["Eureka:Client:Enabled"] = "false"
        }))
      .ConfigureLogging(logging => logging.AddProvider(_logs));

  private sealed class CapturingLoggerProvider : ILoggerProvider
  {
    private readonly ConcurrentQueue<string> _messages = new();

    public IReadOnlyCollection<string> Messages => _messages;

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, _messages);

    public void Dispose()
    {
    }

    private sealed class CapturingLogger(string category, ConcurrentQueue<string> messages) : ILogger
    {
      public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

      public bool IsEnabled(LogLevel logLevel) => true;

      public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        messages.Enqueue($"{category}: {formatter(state, exception)}");
    }
  }
}
