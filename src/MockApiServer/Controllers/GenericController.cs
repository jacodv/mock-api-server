using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockApiServer.Helpers;
using MockApiServer.Services;

namespace MockApiServer.Controllers
{
  [ApiController]
  [Route("{*url}")]
  public class GenericController : MockControllerBase<GenericController>
  {
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IMockDataService _mockDataService;
    private readonly ILogger<GenericController> _logger;

    public GenericController(
      IWebHostEnvironment webHostEnvironment, 
      IMockDataService mockDataService, ILogger<GenericController> logger):
      base(logger, mockDataService)
    {
      _webHostEnvironment = webHostEnvironment;
      _mockDataService = mockDataService;
      _logger = logger;
    }

    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    public async Task<IActionResult> All()
    {
      return await GetResponseFromFile();
    }

    private async Task<IActionResult> GetResponseFromFile()
    {
      string path = Request.Path;
      var method = Request.Method;
      path.ReplaceDoubleSlashes();

      if (path=="/")
        return HomeScreen();

      var razorModel = new Models.RazorModel();
      try
      {
        razorModel.HttpMethod = method;
        razorModel.RequestPath = path;
        razorModel.QueryString = Request.QueryString.ToString();
        razorModel.RequestBody = await _getBodyContentAsStringAsync(Request);
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw;
      }

      if (path=="/graphql")
        return await GetGraphQlResult(razorModel);
      return await GetExpectedResult(method, path, Request.QueryString.HasValue?Request.QueryString.Value:null, razorModel);
    }
    private IActionResult HomeScreen()
    {
      var homeScreen = _mockDataService.GetHomeScreen();
      return Content(homeScreen, "text/html", Encoding.UTF8);
    }
    private static async Task<string> _getBodyContentAsStringAsync(HttpRequest request)
    {
      string content;

      await using (var receiveStream = request.Body)
      {
        using var readStream = new StreamReader(receiveStream);
        content = await readStream.ReadToEndAsync();
      }

      return content;
    }
  }
}
