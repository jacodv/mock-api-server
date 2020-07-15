using System.Text;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MockApiJsonServer.Helpers;
using MockApiJsonServer.Services;

namespace MockApiJsonServer.Controllers
{
  [ApiController]
  [Route("{*url}")]
  public class GenericController : ControllerBase
  {
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileService _fileService;

    public GenericController(IWebHostEnvironment webHostEnvironment, IFileService fileService)
    {
      _webHostEnvironment = webHostEnvironment;
      _fileService = fileService;
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
      var file = GetResponseFile();

      if (string.IsNullOrEmpty(file))
        return HomeScreen();

      var response = _fileService.ReadFile(file);
      if (response == null)
        return NotFound();
      return Ok(response);
    }
    private IActionResult HomeScreen()
    {
      var webRootPath = _webHostEnvironment.WebRootPath;
      var homeScreen = _fileService.GetHomeScreen(webRootPath);
      return Content(homeScreen, "text/html", Encoding.UTF8);
    }
    private string GetResponseFile()
    {
      string path = Request.Path;
      var method = Request.Method;
      path.ReplaceDoubleSlashes();

      if (path == "/")
        return null;

      path = path.Remove("/api/");
      path = path.RemoveLeading('/').RemoveTrailing('/');

      return $"{method.ToLower()}_{path.Replace('/', '-')}.json";
    }


  }
}
