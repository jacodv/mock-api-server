using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
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

    public TestApiSampleResponses(
      TestFixture<Startup> fixture,
      ITestOutputHelper testOutputHelper)
    {
      _fixture = fixture;
      fixture.Init(testOutputHelper);
    }

    [Fact]
    public async Task Get_ApiSample_ShouldReturnModel()
    {
      // Arrange
      const string request = "/api/Sample";

      // Act
      var response = await _fixture.Client.GetAsync(request);

      // Assert
      await _fixture.ValidateSuccessResponse<SampleModel>(response);
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

    [Fact]
    public async Task Get_GivenQueryString_ShouldReturnNotFound()
    {
      // Arrange
      const string request = "/api/Sample?param1=one&param2=two";

      // Act
      var response = await _fixture.Client.GetAsync(request);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_AuthWithRazor_ShouldEvaluateAndReturnModel()
    {

      // Arrange
      const string request = "/auth";
      const string userName = "TestUser";

      var authItems=new List<KeyValuePair<string, string>>()
      {
        new("username", userName),
        new("password", "SomeSecurePassword"),
        new("grant_type", "password"),
        new("client_id", "test-client-id")
      };
      var authData = new FormUrlEncodedContent(authItems);
      var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(request, UriKind.Relative))
      {
        Content = authData
      };

      // Act
      var authResponse = await _fixture.Client.SendAsync(requestMessage);

      // Assert
      var authModel = await _fixture.ValidateSuccessResponse<AuthModel>(authResponse);
      authModel!.UserName.Should().Be(userName);
      authModel.Token.Should().NotBeNullOrEmpty();
      authModel.ExpiresInSeconds.Should().Be(1800);
      var calculatedDiff = authModel.Expires - authModel.Created;
      calculatedDiff.TotalSeconds.Should().Be(authModel.ExpiresInSeconds);
    }

    [Fact]
    public async Task Get_ApiRequest_WithExpectedHeader_AndNoHeaderValue_ShouldValidateHeader()
    {
      // Arrange
      const string request = "/api/ValidateHeaderKey";
      const string headerName = "Authorization";
      var id=Guid.NewGuid().ToString();
      var testCase = new TestCase("GET", request, new SampleModel{ Id=id})
      {
        ExpectedHeaders = new(){{headerName,null}}
      };

      var jsonContent = JsonConvert.SerializeObject(testCase);
      await _fixture.Client.PostAsync("/api/testsetup", new StringContent(jsonContent, MediaTypeHeaderValue.Parse("application/json")));

      // Act
      _fixture.Client.DefaultRequestHeaders.Add(headerName, "some-value");
      var response = await _fixture.Client.GetAsync(request);

      // Assert
      _fixture.Client.DefaultRequestHeaders.Remove(headerName);
      await _fixture.ValidateSuccessResponse<SampleModel>(response);
    }

    [Fact]
    public async Task Get_ApiRequest_WithExpectedHeader_AndHeaderValue_ShouldValidateHeaderKeyAndValue()
    {
      // Arrange
      const string request = "/api/ValidateHeaderKeyAndValue";
      const string headerName = "Authorization";
      const string headerValue = "Bearer token";
      var id=Guid.NewGuid().ToString();
      var testCase = new TestCase("GET", request, new SampleModel{ Id=id})
      {
        ExpectedHeaders = new(){{headerName,headerValue}}
      };

      var jsonContent = JsonConvert.SerializeObject(testCase);
      await _fixture.Client.PostAsync("/api/testsetup", new StringContent(jsonContent, MediaTypeHeaderValue.Parse("application/json")));

      // Act
      _fixture.Client.DefaultRequestHeaders.Add(headerName, headerValue);
      var response = await _fixture.Client.GetAsync(request);

      // Assert
      _fixture.Client.DefaultRequestHeaders.Remove(headerName);
      await _fixture.ValidateSuccessResponse<SampleModel>(response);
    }

    [Fact]
    public async Task Get_ApiRequest_WithExpectedStatusCode_ShouldReturnExpectedStatusCode()
    {
      // Arrange
      const string request = "/api/forbidden";
      var testCase = new TestCase("GET", request, "This request was forbidden")
      {
        ExpectedStatusCode = 403
      };

      var jsonContent = JsonConvert.SerializeObject(testCase);
      await _fixture.Client.PostAsync("/api/testsetup", new StringContent(jsonContent, MediaTypeHeaderValue.Parse("application/json")));

      // Act
      var response = await _fixture.Client.GetAsync(request);

      // Assert
      var content = await response.Content.ReadAsStringAsync();
      _fixture.LogMessage($"{response.StatusCode}-{response.ReasonPhrase}. {content}");
      response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

  }

}
