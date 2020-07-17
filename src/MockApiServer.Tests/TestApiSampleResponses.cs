using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FluentAssertions;
using MockApiServer.Models;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace MockApiServer.Tests
{
  public class TestApiSampleResponses : IClassFixture<TestFixture<Startup>>
  {
    private readonly TestFixture<Startup> _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public TestApiSampleResponses(
      TestFixture<Startup> fixture, 
      ITestOutputHelper testOutputHelper)
    {
      _fixture = fixture;
      _testOutputHelper = testOutputHelper;

      fixture.Init(testOutputHelper);
    }

    [Fact]
    public async Task Get_ApiSample_ShouldReturnModel()
    {
      // Arrange
      var request = "/api/Sample";

      // Act
      var response = await _fixture.Client.GetAsync(request);

      // Assert
      await _fixture.ValidateSuccessResponse<Models.SampleModel>(response);
    }

    [Fact]
    public async Task Get_ApiInvalidUrl_ShouldReturnNotFound()
    {
      // Arrange
      const string request = "/api/ShouldNotExist";

      // Act
      var response = await _fixture.Client.GetAsync(request);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
  }

  public class TestSetupControllerTests : IClassFixture<TestFixture<Startup>>
  {
    private readonly TestFixture<Startup> _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public TestSetupControllerTests(TestFixture<Startup> fixture, 
      ITestOutputHelper testOutputHelper)
    {
      _fixture = fixture;
      _testOutputHelper = testOutputHelper;
      _fixture.Init(_testOutputHelper);
    }

    [Fact]
    public async Task GetAll_GivenNoArguments_ShouldReturnSamples()
    {
      // Arrange
      const string testControllerPath = "/api/TestSetup";

      // Act
      var responseAllAfterCreate =
        await _fixture.Client.GetAsync($"{testControllerPath}");
      
      // Assert
      var resultAllAfterCreate = await _fixture.ValidateSuccessResponse<string[]>(responseAllAfterCreate);
      resultAllAfterCreate.Should().Contain("get_api_sample.json");
    }

    [Fact]
    public async Task CRUD_GivenValidInput_ShouldCreateReadUpdateDelete()
    {
      // Arrange
      const string testControllerPath = "/api/TestSetup";
      const string requestPath = "api/Tests";
      var httpMethod = "GET";
      var newModel = new Models.SampleModel()
      {
        Id = "CrudTestId",
        Name = "CrudTestName"
      };
      var expectedResult = new Models.ExpectedTestResult()
      {
        ExpectedResult = newModel,
        HttpMethod = httpMethod,
        RequestPath = requestPath
      };

      // Act and Assert
      // CREATE
      var createResponse = await _fixture.Client.PostAsync(testControllerPath, _fixture.GetHttpContent(expectedResult));
      createResponse.EnsureSuccessStatusCode();

      //var createUrl = $"{testControllerPath}/{httpMethod}/{HttpUtility.UrlEncode(requestPath)}";
      var createUrl = $"{testControllerPath}/GET/cool";
      _testOutputHelper.WriteLine(createUrl);
      var responseAfterCreate =
        await _fixture.Client.GetAsync(createUrl);
      await _fixture.ValidateSuccessResponse<SampleModel>(responseAfterCreate);

      //var responseAllAfterCreate =
      //  await _fixture.Client.GetAsync($"{testControllerPath}");
      //var resultAllAfterCreate = await _fixture.ValidateSuccessResponse<string[]>(responseAllAfterCreate);
      //resultAllAfterCreate.Should().Contain("CrudTestName");


    }
  }

}
