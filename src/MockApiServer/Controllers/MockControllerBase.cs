using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockApiServer.Models;
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
    protected async Task<IActionResult> GetExpectedResult(string httpMethod, string path, string? queryString=null, RazorModel? razorModel=null)
    {
      try
      {
        var (value, testCase) = await _mockDataService.ReadFile(httpMethod, path, queryString, razorModel);

        if (testCase == null || !testCase.ExpectedHeaders.Any())
          return new OkObjectResult(JsonConvert.DeserializeObject(value: value));
        
        var requestHeaderNames = Request.Headers.Keys;

        foreach (var testCaseExpectedHeader in testCase.ExpectedHeaders)
        {
          if(!Request.Headers.ContainsKey(testCaseExpectedHeader.Key))
            return BadRequest($"Expected header {testCaseExpectedHeader.Key} not found in request [{string.Join(',',requestHeaderNames)}]");
          if (testCaseExpectedHeader.Value != null &&
              !Request.Headers[testCaseExpectedHeader.Key].Contains(testCaseExpectedHeader.Value))
            return BadRequest($"Expected header {testCaseExpectedHeader.Key} with value {testCaseExpectedHeader.Value} not found in request");
        }

        return new OkObjectResult(JsonConvert.DeserializeObject(value: value));
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
    protected async Task<IActionResult> GetGraphQlResult(RazorModel razorModel)
    {
      try
      {
        var graphQlRequest = JsonConvert.DeserializeObject<GraphQlRequest>(razorModel.RequestBody!);
        if(graphQlRequest==null || string.IsNullOrEmpty(graphQlRequest.query))
          throw new InvalidOperationException($"Invalid GraphQLRequest:\n{razorModel.RequestBody}");
        var result = await _mockDataService.ReadGraphQlFile(graphQlRequest);
        return new ObjectResult(JsonConvert.DeserializeObject(value: result));
      }
      catch (FileNotFoundException e)
      {
        _logger.LogInformation($"File not found: {e.FileName}");
        return NotFound(e.Message);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Failed to read and parse content from the file:\n{JsonConvert.SerializeObject(razorModel)}");
        return BadRequest(ex.Message);
      }
    }
  }
}