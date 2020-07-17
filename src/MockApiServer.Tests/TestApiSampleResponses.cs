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
      var request = "/api/Sample";

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
    public async Task Get_GivenQueryString_ShouldReturnnotFound()
    {
      // Arrange
      var request = "/api/Sample?param1=one&param2=two";

      // Act
      var response = await _fixture.Client.GetAsync(request);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
  }
}
