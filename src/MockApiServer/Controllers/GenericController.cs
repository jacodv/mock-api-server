using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
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

      if (path == "/")
        return HomeScreen();

      return await GetExpectedResult(method, path, Request.QueryString.HasValue?Request.QueryString.Value:null);
    }
    private IActionResult HomeScreen()
    {
      var homeScreen = _mockDataService.GetHomeScreen();
      return Content(homeScreen, "text/html", Encoding.UTF8);
    }
  }
}
