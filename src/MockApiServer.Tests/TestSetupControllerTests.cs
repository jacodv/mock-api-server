using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using FluentAssertions;
using MockApiServer.Models;
using Newtonsoft.Json;
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
      var testCase = new TestCase(null,"api/somePath",new { prop = "prop1" });

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
      var testCase = new TestCase("POST", null, new { prop = "prop1" });

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
      var testCase = new TestCase("POST", "api/somePath", null);

      // Act Assert Post
      var invalidResponse = await _fixture.Client.PostAsync(testControllerPath, _fixture.GetHttpContent(testCase));
      invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Post expected bad request for invalid http method");

      // Act Assert Put
      invalidResponse = await _fixture.Client.PutAsync(testControllerPath, _fixture.GetHttpContent(testCase));
      invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Post expected bad request for invalid http method");
    }

    [Fact]
    public async Task PostPut_GivenStaticContentWithoutExtension_ShouldFail()
    {
      // Arrange
      const string testControllerPath = "/api/TestSetup";
      var testCase = new TestCase("POST","api/somePath", "<p/>")
      {
        IsStaticContent = true
      };

      // Act Assert Post
      var invalidResponse = await _fixture.Client.PostAsync(testControllerPath, _fixture.GetHttpContent(testCase));
      invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Post expected bad request for invalid http method");

      // Act Assert Put
      invalidResponse = await _fixture.Client.PutAsync(testControllerPath, _fixture.GetHttpContent(testCase));
      invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Post expected bad request for invalid http method");
    }
    [Fact]
    public async Task PostPut_GivenStaticContentWithInvalidBinary_ShouldFail()
    {
      // Arrange
      const string testControllerPath = "/api/TestSetup";
      var testCase = new TestCase("POST","api/somePath","<p/>")
      {
        IsStaticContent = true,
        StaticContentExtension = "gif",
        IsBinary = true,
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
      var testCase = new TestCase(httpMethod, requestPath, newModel);

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
    public async Task CRUD_GivenValidStaticHtmlContent_ShouldCreateReadUpdateDelete()
    {
      // Arrange
      const string requestPath = "api/samplehtml.html";
      var httpMethod = "GET";
      var content = "<p>Static Html Content</p>";
      var expectedFileName = "get_api_samplehtml.html";
      var testCase = new TestCase(httpMethod, requestPath, content)
      {
        IsStaticContent =true,
        IsBinary = false,
        StaticContentExtension = "html"
      };

      var itemUrl = $"{TestControllerPath}/{httpMethod}?path={HttpUtility.UrlEncode(requestPath)}";

      // Act and Assert
      // CREATE
      var createResponse = await _fixture.Client.PostAsync(TestControllerPath, _fixture.GetHttpContent(testCase));
      createResponse.EnsureSuccessStatusCode();
      // CREATE READ
      await _validateReadItemAndAllItems<string>(TestControllerPath, itemUrl, expectedFileName);

      // UPDATE
      var updatedModel = "<p>Updated Html</p>";
      testCase.ExpectedResult = updatedModel;
      var updateResponse = await _fixture.Client.PutAsync(TestControllerPath, _fixture.GetHttpContent(testCase));
      updateResponse.EnsureSuccessStatusCode();
      // UPDATE READ
      await _validateReadItemAndAllItems<string>(TestControllerPath, itemUrl, expectedFileName);

      // DELETE
      var deleteResponse = await _fixture.Client.DeleteAsync(itemUrl);
      deleteResponse.EnsureSuccessStatusCode();
      // DELETE READ
      var readAfterDelete = await _fixture.Client.GetAsync(itemUrl);
      readAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CRD_GivenValidStaticHtmlContent_ShouldCreateReadUpdateDelete()
    {
      // Arrange
      const string requestPath = "api/loader.gif";
      var httpMethod = "GET";
      var loaderGifData = File.ReadAllBytes("./Resources/loader.gif");
      var content = Convert.ToBase64String(loaderGifData);
      var expectedFileName = "get_api_loader.gif";
      var testCase = new TestCase(httpMethod, requestPath,content)
      {
        IsStaticContent = true,
        IsBinary = true,
        StaticContentExtension = "gif"
      };

      var itemUrl = $"{TestControllerPath}/{httpMethod}?path={HttpUtility.UrlEncode(requestPath)}";

      // Act and Assert
      // CREATE
      var createResponse = await _fixture.Client.PostAsync(TestControllerPath, _fixture.GetHttpContent(testCase));
      createResponse.EnsureSuccessStatusCode();
      // CREATE READ
      await _validateFileDataAndAllItems(TestControllerPath, itemUrl, expectedFileName);

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
      const string requestPath = "api/CRUDTests";
      const string expectedFileName = "get_api_crudtests";
      const string queryString = "?param1=one&param2=two";
      var httpMethod = "GET";
      var newModel = new SampleModel()
      {
        Id = "CrudTestId",
        Name = "CrudTestName"
      };
      var testCase = new TestCase(httpMethod,requestPath,newModel)
      {
        QueryString = queryString
      };

      var itemUrl = $"{TestControllerPath}/{httpMethod}?path={HttpUtility.UrlEncode(requestPath)}&queryString={HttpUtility.UrlEncode(queryString)}";

      // Act and Assert
      // CREATE
      var createResponse = await _fixture.Client.PostAsync(TestControllerPath, _fixture.GetHttpContent(testCase));
      createResponse.EnsureSuccessStatusCode();
      // CREATE READ
      await _validateReadItemAndAllItems(TestControllerPath, itemUrl, expectedFileName);

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
      await _validateReadItemAndAllItems(TestControllerPath, itemUrl, expectedFileName);

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
      const string requestPath = "api/CrudTestsWithQueryString";
      const string expectedFileName = "get_api_crudtestswithquerystring";
      const string queryString = "?param1=one&param2=two";
      const string httpMethod = "GET";
      var newModel = new SampleModel()
      {
        Id = "CrudTestId",
        Name = "CrudTestName"
      };
      var testCase = new TestCase(httpMethod,requestPath,newModel)
      {
        QueryString = queryString,
        IsRazorFile = true
      };

      var itemUrl = $"{TestControllerPath}/{httpMethod}?path={HttpUtility.UrlEncode(requestPath)}&queryString={HttpUtility.UrlEncode(queryString)}";

      // Act and Assert
      // CREATE
      var createResponse = await _fixture.Client.PostAsync(TestControllerPath, _fixture.GetHttpContent(testCase));
      createResponse.EnsureSuccessStatusCode();
      // CREATE READ
      await _validateReadItemAndAllItems(TestControllerPath, itemUrl, expectedFileName);

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
      await _validateReadItemAndAllItems(TestControllerPath, itemUrl, expectedFileName);

      // DELETE
      var deleteResponse = await _fixture.Client.DeleteAsync(itemUrl);
      deleteResponse.EnsureSuccessStatusCode();
      // DELETE READ
      var readAfterDelete = await _fixture.Client.GetAsync(itemUrl);
      readAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CRUD_GivenValidGraphQL_ShouldCreateReadUpdateDelete()
    {
      // Arrange
      const string query = @"{   
	                              sample(first:3)
                                    {
                                      nodes{
                                        id
                                      }
                                  }
                              }";
      const string result = @"
                              {
                                data: {
                                    sample: {
                                      nodes: [
                                      {
                                        id: ""Id1""
                                      },
                                      {
                                        id: ""Id2""
                                      },
                                      {
                                        id: ""Id3""
                                      }
                                      ]
                                    }
                                  },
                                  extensions: {}
                                }";

      var testCase = new GraphQlTestCase(query, "GetSamples", JsonConvert.DeserializeObject(result)!);

      // Act and Assert
      // CREATE
      var createResponse = await _fixture.Client.PostAsync($"{TestControllerPath}/GraphQlSetup", _fixture.GetHttpContent(testCase));
      createResponse.EnsureSuccessStatusCode();
      // CREATE READ
      var fileName = await _validateAllItems(TestControllerPath, testCase.OperationName.ToLower());


      // DELETE
      var deleteResponse = await _fixture.Client.DeleteAsync($"{TestControllerPath}/GraphQlDelete?fileName={HttpUtility.UrlEncode(fileName)}");
      deleteResponse.EnsureSuccessStatusCode();
    }

    #region Private
    private async Task _validateReadItemAndAllItems(string testControllerPath, string getItemUrl, string fileName = "get_api_tests")
    {
      await _validateReadItemAndAllItems<SampleModel>(testControllerPath, getItemUrl, fileName);
    }
    private async Task _validateReadItemAndAllItems<T>(string testControllerPath, string getItemUrl, string fileName)
    {
      var response = await _fixture.Client.GetAsync(getItemUrl);
      await _fixture.ValidateSuccessResponse<T>(response);
      await _validateAllItems(testControllerPath, fileName);
    }
    private async Task _validateFileDataAndAllItems(string testControllerPath, string getItemUrl, string fileName, byte[] resultsToCompare = null)
    {
      var response = await _fixture.Client.GetAsync(getItemUrl);

      var fileData = await _fixture.ValidateSuccessFile(response);
      if (resultsToCompare != null) 
        fileData.Should().BeSameAs(resultsToCompare);
      await _validateAllItems(testControllerPath, fileName);
    }
    private async Task<string> _validateAllItems(string testControllerPath, string fileName)
    {
      var responseAll =
        await _fixture.Client.GetAsync($"{testControllerPath}");
      var resultAll = await _fixture.ValidateSuccessResponse<string[]>(responseAll);

      var foundItem = resultAll.FirstOrDefault(x => x.Contains(fileName));
      foundItem.Should().NotBeNullOrEmpty($"{fileName}: But found: {string.Join(",\n", resultAll)}");
      return foundItem;
    }
    #endregion
  }
}