using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockApiServer.Services;
using Newtonsoft.Json;

namespace MockApiServer.Controllers
{
  public abstract class MockControllerBase<T> : ControllerBase
  {
    private readonly ILogger<T> _logger;
    private readonly IMockDataService _mockDataService;

    protected MockControllerBase(ILogger<T> logger, IMockDataService mockDataService)
    {
      _logger = logger;
      _mockDataService = mockDataService;
    }
    protected async Task<IActionResult> GetExpectedResult(string httpMethod, string path, string queryString=null)
    {
      try
      {
        return new OkObjectResult(JsonConvert.DeserializeObject(value: await _mockDataService.ReadFile(httpMethod, path, queryString)));
      }
      catch (FileNotFoundException e)
      {
        _logger.LogInformation($"File not found: {e.FileName}");
        return NotFound(e.Message);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Failed to read and parse content from the file: {path}");
        return BadRequest(ex.Message);
      }
    }
  }
}