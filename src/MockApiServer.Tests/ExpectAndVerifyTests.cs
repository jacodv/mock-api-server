using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using MockApiServer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
      var testCase = new TestCase(method, request, new
      {
        Field1 = "field1",
        Field2 = "field2"
      });

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
      var testCase = new TestCase(method, request, new
      {
        Field1 = "field1",
        Field2 = "field2"
      });

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

    [Fact]
    public async Task ExpectOne_GiveValidGraphQlSetupAndValidRequest_ShouldVerify()
    {
      // Arrange
      const int expectedCount=1;
      const string query = @"{   
	                              expect(first:3)
                                    {
                                      nodes{
                                        id
                                      }
                                  }
                              }";
      const string result = @"
                              {
                                data: {
                                    expect: {
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

      var testCase = new GraphQlTestCase(operationName:"ExpectSamples",query:query,expectedResult: JsonConvert.DeserializeObject(result)!);

      // Act Setup
      var setupResponse = await _fixture.Client.PostAsync(
        $"{SetupControllerPath}/GraphQlExpectation",
        _fixture.GetHttpContent(testCase));
      setupResponse.EnsureSuccessStatusCode();

      // Act Test
      JObject graphResult = await _fixture.ExecuteGraphQlQuery(testCase.Query, testCase.OperationName);
      graphResult["expect"].Should().NotBeNull();

      // Assert
      var verifyResponse = 
        await _fixture.Client.PostAsync($"{SetupControllerPath}/GraphQlExpect/{expectedCount}", _fixture.GetHttpContent(testCase));
      verifyResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ExpectOne_GiveValidGraphQlSetupAndValidRequest_ShouldNotVerify()
    {
      // Arrange
      const int expectedCount = 1;
      const string query = @"{   
	                              expect(first:3)
                                    {
                                      nodes{
                                        id
                                      }
                                  }
                              }";
      const string result = @"
                              {
                                data: {
                                    expect: {
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

      var testCase = new GraphQlTestCase(query, "ExpectSamples", JsonConvert.DeserializeObject(result)!);

      // Act Setup
      var setupResponse = await _fixture.Client.PostAsync(
        $"{SetupControllerPath}/GraphQlExpectation",
        _fixture.GetHttpContent(testCase));
      setupResponse.EnsureSuccessStatusCode();

      // Assert
      var verifyResponse =
        await _fixture.Client.PostAsync($"{SetupControllerPath}/GraphQlExpect/{expectedCount}", _fixture.GetHttpContent(testCase));

      verifyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
      var error = await verifyResponse.Content.ReadAsStringAsync();
      error.Should().Be($"\"Expected: 1 but executed: 0\"");
    }

  }
}