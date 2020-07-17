using System.Collections.Generic;
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
      return await _mockDataService.GetPersistedFileNames();
    }

    [HttpGet("{method}/{path}")]
    public Task<IActionResult> Get(string method, string path)
    {
      return GetExpectedResult(method, path);
    }

    [HttpPost]
    [HttpPut]
    public async Task<IActionResult> PostPut([FromBody] ExpectedTestResult expectedResult)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      await _mockDataService.WriteFile(expectedResult);

      return Ok();
    }


    [HttpDelete]
    public async Task<IActionResult> Delete(string method, string path)
    {
      await _mockDataService.DeleteFile(method, path);
      return Ok();
    }
  }
}
