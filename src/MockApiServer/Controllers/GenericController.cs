using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockApiServer.Helpers;
using MockApiServer.Services;

namespace MockApiServer.Controllers
{
  /// <summary>
  /// This controller is used to handle all the requests that are not handled by any other controller.  It serves are a catch all.
  /// </summary>
  [ApiController]
  [Route("{*url}")]
  public class GenericController : MockControllerBase<GenericController>
  {
    private readonly IMockDataService _mockDataService;
    private readonly ILogger<GenericController> _logger;

    public GenericController(IMockDataService mockDataService, ILogger<GenericController> logger):
      base(logger, mockDataService)
    {
      _mockDataService = mockDataService;
      _logger = logger;
    }

    /// <summary>
    /// Accepts all requests and returns a valid response of the path is matched in a setup.
    /// </summary>
    /// <returns><see cref="IActionResult"/>Content as defined in the <see cref="Models.TestCase"/></returns>
    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    // ReSharper disable once RouteTemplates.MethodMissingRouteParameters
    public async Task<IActionResult> All()
    {
      try
      {
        return await _getResponseFromFile();
      }
      catch (Exception e)
      {
        _logger.LogError(e, "Error in GenericController.All");
        throw;
      }
    }

    #region Private
    private async Task<IActionResult> _getResponseFromFile()
    {
      string path = Request.Path;
      var method = Request.Method;
      path.ReplaceDoubleSlashes();

      if (path=="/")
        return _getHomeScreen();

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
    private IActionResult _getHomeScreen()
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
    #endregion
  }
}
