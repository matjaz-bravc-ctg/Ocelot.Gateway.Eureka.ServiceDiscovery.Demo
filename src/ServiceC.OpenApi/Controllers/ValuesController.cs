using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServiceC.OpenApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class ValuesController(ILogger<ValuesController> logger) : ControllerBase
{
  private static readonly string ServiceName = typeof(ValuesController).Assembly.GetName().Name ?? "ServiceC.OpenApi";

  [HttpGet("badcode")]
  public string BadCode() => throw new ArgumentException("Some bad code was executed!");

  [HttpGet]
  [ProducesResponseType<string>(StatusCodes.Status200OK)]
  public IActionResult Get()
  {
    string msg = $"{ServiceName}, Url: {Request.Scheme}://{Request.Host}, Method: {Request.Method}, Path: {Request.Path} -> Value";
    logger.LogInformation("{Message}", msg);
    return Ok(msg);
  }

  [HttpGet("status")]
  [ProducesResponseType<string>(StatusCodes.Status200OK)]
  public IActionResult Status()
  {
    string msg = $"Running on {Request.Host}";
    logger.LogInformation("{Message}", msg);
    return Ok(msg);
  }
}
