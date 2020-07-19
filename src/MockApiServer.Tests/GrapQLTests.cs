using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
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
      JObject result = await _executeQuery(query, operationName);

      // Assert
      _testOutputHelper.WriteLine($"{result["sample"]}");
      result["sample"].Should().NotBeNull();

    }

    private async Task<dynamic> _executeQuery(string query, string operationName)
    {
      var graphQlRequest = new GraphQLRequest(query, null, operationName);
      var graphQlResponse = await _fixture.GqlClient.SendQueryAsync<dynamic>(graphQlRequest, CancellationToken.None);

      if (!(graphQlResponse.Errors?.Length > 0)) 
        return graphQlResponse.Data;

      foreach (var err in graphQlResponse.Errors)
        _testOutputHelper.WriteLine("ERROR: " + JsonConvert.SerializeObject(err));
      throw new InvalidOperationException(graphQlResponse.Errors.First().Message);
    }

    public static bool IsPropertyExist(dynamic result, string name)
    {
      if (result is ExpandoObject)
        return ((IDictionary<string, object>)result).ContainsKey(name);

      return result.GetType().GetProperty(name) != null;
    }
  }
}
