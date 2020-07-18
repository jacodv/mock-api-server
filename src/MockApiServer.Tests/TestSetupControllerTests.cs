using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using FluentAssertions;
using MockApiServer.Models;
using Xunit;
using Xunit.Abstractions;

namespace MockApiServer.Tests
{
  public class TestSetupControllerTests : IClassFixture<TestFixture<Startup>>
  {
    private const string TestControllerPath = "/api/TestSetup";
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
    public async Task Get_GivenInvalidRequestPath_ShouldReturn_NotFound()
    {
      // Arrange
      var itemUrl = $"{TestControllerPath}/get?path={HttpUtility.UrlEncode("api/invalidPath")}";

      // Act
      var invalidResponse = await _fixture.Client.GetAsync(itemUrl);

      // Assert
      invalidResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_GivenInvalidRequestPath_ShouldReturn_NotFound()
    {
      // Arrange
      var itemUrl = $"{TestControllerPath}/get?path={HttpUtility.UrlEncode("api/invalidPath")}";

      // Act
      var invalidResponse = await _fixture.Client.DeleteAsync(itemUrl);

      // Assert
      invalidResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostPut_GivenInvalidMethod_ShouldFail()
    {
      // Arrange
      const string testControllerPath = "/api/TestSetup";
      var testCase = new ExpectedTestResult()
      {
        HttpMethod = null,
        RequestPath = "api/somePath",
        ExpectedResult = new { prop="prop1"}
      };

      // Act Assert Post
      var invalidResponse = await _fixture.Client.PostAsync(testControllerPath, _fixture.GetHttpContent(testCase));
      invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Post expected bad request for invalid http method");

      // Act Assert Put
      invalidResponse = await _fixture.Client.PutAsync(testControllerPath, _fixture.GetHttpContent(testCase));
      invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Post expected bad request for invalid http method");
    }

    [Fact]
    public async Task PostPut_GivenInvalidRequestPath_ShouldFail()
    {
      // Arrange
      const string testControllerPath = "/api/TestSetup";
      var testCase = new ExpectedTestResult()
      {
        HttpMethod = "POST",
        RequestPath = null,
        ExpectedResult = new { prop = "prop1" }
      };

      // Act Assert Post
      var invalidResponse = await _fixture.Client.PostAsync(testControllerPath, _fixture.GetHttpContent(testCase));
      invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Post expected bad request for invalid http method");

      // Act Assert Put
      invalidResponse = await _fixture.Client.PutAsync(testControllerPath, _fixture.GetHttpContent(testCase));
      invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Post expected bad request for invalid http method");
    }

    [Fact]
    public async Task PostPut_GivenInvalidExpectedResult_ShouldFail()
    {
      // Arrange
      const string testControllerPath = "/api/TestSetup";
      var testCase = new ExpectedTestResult()
      {
        HttpMethod = "POST",
        RequestPath = "api/somePath",
        ExpectedResult = null
      };

      // Act Assert Post
      var invalidResponse = await _fixture.Client.PostAsync(testControllerPath, _fixture.GetHttpContent(testCase));
      invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Post expected bad request for invalid http method");

      // Act Assert Put
      invalidResponse = await _fixture.Client.PutAsync(testControllerPath, _fixture.GetHttpContent(testCase));
      invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Post expected bad request for invalid http method");
    }

    [Fact]
    public async Task CRUD_GivenValidInput_ShouldCreateReadUpdateDelete()
    {
      // Arrange
      const string requestPath = "api/Tests";
      var httpMethod = "GET";
      var newModel = new SampleModel()
      {
        Id = "CrudTestId",
        Name = "CrudTestName"
      };
      var testCase = new ExpectedTestResult()
      {
        ExpectedResult = newModel,
        HttpMethod = httpMethod,
        RequestPath = requestPath
      };

      var itemUrl = $"{TestControllerPath}/{httpMethod}?path={HttpUtility.UrlEncode(requestPath)}";

      // Act and Assert
      // CREATE
      var createResponse = await _fixture.Client.PostAsync(TestControllerPath, _fixture.GetHttpContent(testCase));
      createResponse.EnsureSuccessStatusCode();
      // CREATE READ
      await _validateReadItemAndAllItems(TestControllerPath, itemUrl);

      // UPDATE
      var updatedModel = new SampleModel()
      {
        Id = "CrudTestIdUpdated",
        Name = "CrudTestNameUpdated"
      };
      testCase.ExpectedResult = updatedModel;
      var updateResponse = await _fixture.Client.PutAsync(TestControllerPath, _fixture.GetHttpContent(testCase));
      updateResponse.EnsureSuccessStatusCode();
      // UPDATE READ
      await _validateReadItemAndAllItems(TestControllerPath, itemUrl);

      // DELETE
      var deleteResponse = await _fixture.Client.DeleteAsync(itemUrl);
      deleteResponse.EnsureSuccessStatusCode();
      // DELETE READ
      var readAfterDelete = await _fixture.Client.GetAsync(itemUrl);
      readAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CRUD_GivenValidInputWithQueryString_ShouldCreateReadUpdateDelete()
    {
      // Arrange
      const string requestPath = "api/Tests";
      const string queryString = "?param1=one&param2=two";
      var httpMethod = "GET";
      var newModel = new SampleModel()
      {
        Id = "CrudTestId",
        Name = "CrudTestName"
      };
      var testCase = new ExpectedTestResult()
      {
        ExpectedResult = newModel,
        HttpMethod = httpMethod,
        RequestPath = requestPath,
        QueryString = queryString
      };

      var itemUrl = $"{TestControllerPath}/{httpMethod}?path={HttpUtility.UrlEncode(requestPath)}&queryString={HttpUtility.UrlEncode(queryString)}";

      // Act and Assert
      // CREATE
      var createResponse = await _fixture.Client.PostAsync(TestControllerPath, _fixture.GetHttpContent(testCase));
      createResponse.EnsureSuccessStatusCode();
      // CREATE READ
      await _validateReadItemAndAllItems(TestControllerPath, itemUrl);

      // UPDATE
      var updatedModel = new SampleModel()
      {
        Id = "CrudTestIdUpdated",
        Name = "CrudTestNameUpdated"
      };
      testCase.ExpectedResult = updatedModel;
      var updateResponse = await _fixture.Client.PutAsync(TestControllerPath, _fixture.GetHttpContent(testCase));
      updateResponse.EnsureSuccessStatusCode();
      // UPDATE READ
      await _validateReadItemAndAllItems(TestControllerPath, itemUrl);

      // DELETE
      var deleteResponse = await _fixture.Client.DeleteAsync(itemUrl);
      deleteResponse.EnsureSuccessStatusCode();
      // DELETE READ
      var readAfterDelete = await _fixture.Client.GetAsync(itemUrl);
      readAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CRUD_GivenValidInputWithQueryStringAndRazor_ShouldCreateReadUpdateDelete()
    {
      // Arrange
      const string requestPath = "api/Tests";
      const string queryString = "?param1=one&param2=two";
      const string httpMethod = "GET";
      var newModel = new SampleModel()
      {
        Id = "CrudTestId",
        Name = "CrudTestName"
      };
      var testCase = new ExpectedTestResult()
      {
        ExpectedResult = newModel,
        HttpMethod = httpMethod,
        RequestPath = requestPath,
        QueryString = queryString,
        IsRazorFile = true
      };

      var itemUrl = $"{TestControllerPath}/{httpMethod}?path={HttpUtility.UrlEncode(requestPath)}&queryString={HttpUtility.UrlEncode(queryString)}";

      // Act and Assert
      // CREATE
      var createResponse = await _fixture.Client.PostAsync(TestControllerPath, _fixture.GetHttpContent(testCase));
      createResponse.EnsureSuccessStatusCode();
      // CREATE READ
      await _validateReadItemAndAllItems(TestControllerPath, itemUrl);

      // UPDATE
      var updatedModel = new SampleModel()
      {
        Id = "CrudTestIdUpdated",
        Name = "CrudTestNameUpdated"
      };
      testCase.ExpectedResult = updatedModel;
      var updateResponse = await _fixture.Client.PutAsync(TestControllerPath, _fixture.GetHttpContent(testCase));
      updateResponse.EnsureSuccessStatusCode();
      // UPDATE READ
      await _validateReadItemAndAllItems(TestControllerPath, itemUrl);

      // DELETE
      var deleteResponse = await _fixture.Client.DeleteAsync(itemUrl);
      deleteResponse.EnsureSuccessStatusCode();
      // DELETE READ
      var readAfterDelete = await _fixture.Client.GetAsync(itemUrl);
      readAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #region Private
    private async Task _validateReadItemAndAllItems(string testControllerPath, string getItemUrl, string fileName = "get_api_tests")
    {
      var response = await _fixture.Client.GetAsync(getItemUrl);
      await _fixture.ValidateSuccessResponse<SampleModel>(response);

      var responseAll =
        await _fixture.Client.GetAsync($"{testControllerPath}");
      var resultAll = await _fixture.ValidateSuccessResponse<string[]>(responseAll);
      resultAll.Any(x => x.Contains(fileName))
        .Should()
        .BeTrue($"But found: {string.Join(',', resultAll)}");
    }
    #endregion
  }

  public class ExpectAndVerifyTests : IClassFixture<TestFixture<Startup>>
  {
    private const string SetupControllerPath = "/api/TestSetup";

    private readonly TestFixture<Startup> _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public ExpectAndVerifyTests(TestFixture<Startup> fixture,
      ITestOutputHelper testOutputHelper)
    {
      _fixture = fixture;
      _testOutputHelper = testOutputHelper;
      _fixture.Init(_testOutputHelper);
    }

    [Fact]
    public async Task ExpectOne_GiveValidSetupAndValidRequest_ShouldVerify()
    {
      // Arrange
      const string request = "api/ExpectOneTest";
      const string method = "POST";
      const int expectedCount = 1;
      var testCase = new ExpectedTestResult(){
        RequestPath = request,
        HttpMethod = method,
        ExpectedResult = new
        {
          Field1 = "field1",
          Field2 = "field2"
        }
      };

      // Act Setup
      var setupResponse = await _fixture.Client.PostAsync(
        $"{SetupControllerPath}/SetupExpectation",
        _fixture.GetHttpContent(testCase));
      setupResponse.EnsureSuccessStatusCode();
      
      // Act Test
      var testResponse = await _fixture.Client.PostAsync(
        request, 
        new StringContent("{TestItem:'testItem'}"));
      testResponse.EnsureSuccessStatusCode();

      // Assert
      var verifyResponse = await _fixture.Client.GetAsync(
          $"{SetupControllerPath}/Expect/{expectedCount}/{method}?path={request}&queryString=");
      verifyResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ExpectOne_GiveValidSetupAndNoRequest_ShouldNotVerify()
    {
      // Arrange
      const string request = "api/ExpectOneTest";
      const string method = "POST";
      const int expectedCount = 1;
      var testCase = new ExpectedTestResult()
      {
        RequestPath = request,
        HttpMethod = method,
        ExpectedResult = new
        {
          Field1 = "field1",
          Field2 = "field2"
        }
      };

      // Act Setup
      var setupResponse = await _fixture.Client.PostAsync(
        $"{SetupControllerPath}/SetupExpectation",
        _fixture.GetHttpContent(testCase));
      setupResponse.EnsureSuccessStatusCode();

      // Assert
      var verifyResponse = await _fixture.Client.GetAsync(
        $"{SetupControllerPath}/Expect/{expectedCount}/{method}?path={request}&queryString=");
      verifyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
      var error = await verifyResponse.Content.ReadAsStringAsync();
      error.Should().Be($"\"Expected: 1 but executed: 0\"");
    }
  }
}