using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using MockApiServer.Models;
using Xunit;
using Xunit.Abstractions;

namespace MockApiServer.Tests
{
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
      var testCase = new TestCase(){
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
      var testCase = new TestCase()
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