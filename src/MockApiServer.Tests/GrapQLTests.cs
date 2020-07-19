using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace MockApiServer.Tests
{
  // ReSharper disable once InconsistentNaming
  public class GraphQLTests : IClassFixture<TestFixture<Startup>>
  {
    private readonly TestFixture<Startup> _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public GraphQLTests(
      TestFixture<Startup> fixture,
      ITestOutputHelper testOutputHelper)
    {
      _fixture = fixture;
      _testOutputHelper = testOutputHelper;

      fixture.Init(testOutputHelper);
    }

    [Fact]
    public async Task Query_GivenSampleQuery_ShouldReturnModel()
    {
      // Arrange
      const string query = @"query {
                                sample {
                                  nodes {
                                    id
                                  }
                                }
                              }";
      const string operationName = "SampleQuery";
      // Act
      JObject result = await _fixture.ExecuteGraphQlQuery(query, operationName);

      // Assert
      _testOutputHelper.WriteLine($"{result["sample"]}");
      result["sample"].Should().NotBeNull();

    }
  }
}
