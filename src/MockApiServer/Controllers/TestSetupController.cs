using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockApiServer.Models;
using MockApiServer.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MockApiServer.Controllers
{
  [ApiController]
  [Route("api/TestSetup")]
  public class TestSetupController : MockControllerBase<TestSetupController>
  {
    private readonly IMockDataService _mockDataService;
    private readonly ILogger<TestSetupController> _logger;

    public TestSetupController(IMockDataService mockDataService, ILogger<TestSetupController> logger):
      base(logger, mockDataService)
    {
      _mockDataService = mockDataService;
      _logger = logger;
    }

    // GET: api/<TestSetupController>
    [HttpGet]
    public async Task<IEnumerable<string>> Get()
    {
      var result = new List<string>();
      result.AddRange(_mockDataService.GetExpectationKeys());
      result.AddRange(await _mockDataService.GetPersistedFileNames());
      return result;
    }

    [HttpGet("{method}")]
    public Task<IActionResult> Get(string method, [FromQuery]string path, [FromQuery] string queryString)
    {
      _logger.LogDebug($"Method: ${method}, Path: {path}");
      return GetExpectedResult(method, path, queryString);
    }

    [HttpPost]
    [HttpPut]
    public async Task<IActionResult> PostPut([FromBody] ExpectedTestResult testCase)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      await _mockDataService.WriteFile(testCase);

      return Ok();
    }

    [HttpDelete("{method}")]
    public async Task<IActionResult> Delete(string method, [FromQuery]string path, [FromQuery] string queryString)
    {
      try
      {
        await _mockDataService.DeleteFile(method, path, queryString);
      }
      catch (FileNotFoundException)
      {
        return NotFound($"{method}?path={path}");
      }
      return Ok();
    }

    [Route("SetupExpectation")]
    [HttpPut]
    [HttpPost]
    public IActionResult SetupExpectation([FromBody] ExpectedTestResult testCase)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      _mockDataService.SetupExpectation(testCase);

      return Ok();
    }

    [Route("Expect/{count}/{method}")]
    [HttpGet]
    public IActionResult Expect(string method, int count, [FromQuery] string path, [FromQuery] string queryString)
    {
      var actual = _mockDataService.Expect(method, count, path, queryString);
      if (actual == count)
        return Ok();
      return BadRequest($"Expected: {count} but executed: {actual}");
    }
  }
}
