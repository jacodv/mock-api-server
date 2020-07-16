using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockApiServer.Helpers;
using MockApiServer.Services;
using Newtonsoft.Json;

namespace MockApiServer.Controllers
{
  [ApiController]
  [Route("{*url}")]
  public class GenericController : ControllerBase
  {
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IMockDataService _mockDataService;
    private readonly ILogger<GenericController> _logger;

    public GenericController(
      IWebHostEnvironment webHostEnvironment, 
      IMockDataService mockDataService, ILogger<GenericController> logger)
    {
      _webHostEnvironment = webHostEnvironment;
      _mockDataService = mockDataService;
      _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
      return GetResponseFromFile();
    }

    [HttpPost]
    public IActionResult Post()
    {
      return GetResponseFromFile();
    }

    [HttpPut]
    public IActionResult Put()
    {
      return GetResponseFromFile();
    }

    [HttpDelete]
    public IActionResult Delete()
    {
      return GetResponseFromFile();
    }

    private IActionResult GetResponseFromFile()
    {
      string path = Request.Path;
      var method = Request.Method;
      path.ReplaceDoubleSlashes();

      if (path == "/")
        return HomeScreen();

      try
      {
        return new OkObjectResult(JsonConvert.DeserializeObject(_mockDataService.ReadFile(method, path)));
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
    private IActionResult HomeScreen()
    {
      var homeScreen = _mockDataService.GetHomeScreen();
      return Content(homeScreen, "text/html", Encoding.UTF8);
    }
  }
}
