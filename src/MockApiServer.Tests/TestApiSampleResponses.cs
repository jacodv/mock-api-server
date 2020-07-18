using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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

      List<KeyValuePair<string,string>> authItems=new List<KeyValuePair<string, string>>()
      {
        new KeyValuePair<string, string>("username", userName),
        new KeyValuePair<string, string>("password", "SomeSecurePassword"),
        new KeyValuePair<string, string>("grant_type", "password"),
        new KeyValuePair<string, string>("client_id", "test-client-id")
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
      authModel.UserName.Should().Be(userName);
      authModel.Token.Should().NotBeNullOrEmpty();
      authModel.ExpiresInSeconds.Should().Be(1800);
      var calculatedDiff = authModel.Expires - authModel.Created;
      calculatedDiff.TotalSeconds.Should().Be(authModel.ExpiresInSeconds);
    }
  }

}
