using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using Newtonsoft.Json;
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
      var response = await _executeQuery(query, operationName);

      // Assert
      response.Length.Should().BeGreaterThan(0);
    }

    private async Task<string> _executeQuery(string query, string operationName)
    {
      var graphQlRequest = new GraphQLRequest(query, null, operationName);
      var graphQlResponse = await _fixture.GqlClient.SendQueryAsync<string>(graphQlRequest);

      if (!(graphQlResponse.Errors?.Length > 0)) 
        return graphQlResponse.Data;

      foreach (var err in graphQlResponse.Errors)
        _testOutputHelper.WriteLine("ERROR: " + JsonConvert.SerializeObject(err));
      throw new InvalidOperationException(graphQlResponse.Errors.First().Message);
    }
  }
}
