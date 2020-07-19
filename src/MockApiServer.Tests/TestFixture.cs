using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockApiServer.Models;
using Newtonsoft.Json;
using Serilog;
using Xunit.Abstractions;

namespace MockApiServer.Tests
{
  public class TestFixture<TStartup> : IDisposable
  {
    private readonly TestServer _testServer;
    private ITestOutputHelper _testOutputHelper;

    public static string GetProjectPath(string projectRelativePath, Assembly startupAssembly)
    {
      var projectName = startupAssembly.GetName().Name;

      var applicationBasePath = AppContext.BaseDirectory;

      var directoryInfo = new DirectoryInfo(applicationBasePath);

      do
      {
        directoryInfo = directoryInfo.Parent;

        var projectDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, projectRelativePath));

        if (projectDirectoryInfo.Exists)
        {
          var fileName = Path.Combine(projectDirectoryInfo.FullName, projectName, $"{projectName}.csproj");

          if (new FileInfo(fileName).Exists)
            return Path.Combine(projectDirectoryInfo.FullName, projectName);
        }
      }
      while (directoryInfo.Parent != null);

      throw new Exception($"Project root could not be located using the application root {applicationBasePath}.");
    }


    public TestFixture()
      : this(Path.Combine(""))
    {
      Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
    }
    protected TestFixture(string relativeTargetProjectParentDir)
    {
      var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;
      var contentRoot = GetProjectPath(relativeTargetProjectParentDir, startupAssembly);

      var configurationBuilder = new ConfigurationBuilder()
        .SetBasePath(contentRoot)
        .AddJsonFile("appsettings.json");

      var webHostBuilder = new WebHostBuilder()
        .UseContentRoot(contentRoot)
        .ConfigureServices(InitializeServices)
        .UseConfiguration(configurationBuilder.Build())
        .UseEnvironment("Development")
        .UseStartup(typeof(TStartup));

      // Create instance of test server
      _testServer = new TestServer(webHostBuilder);

      // Add configuration for client
      Client = _testServer.CreateClient();
      Client.BaseAddress = new Uri("http://localhost:3001");
      Client.DefaultRequestHeaders.Accept.Clear();
      Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

      // ReSharper disable once InconsistentNaming
      var graphQLEndPoint = new Uri("/graphql",UriKind.Relative);
      GqlClient = new GraphQLHttpClient(
        options:new GraphQLHttpClientOptions(){EndPoint = graphQLEndPoint}, 
        serializer: new NewtonsoftJsonSerializer(),
        Client);
    }

    public HttpClient Client { get; }
    public GraphQLHttpClient GqlClient { get; }

    protected virtual void InitializeServices(IServiceCollection services)
    {
      var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;

      var manager = new ApplicationPartManager
      {
        ApplicationParts =
        {
          new AssemblyPart(startupAssembly)
        },
        FeatureProviders =
        {
          new ControllerFeatureProvider(),
        }
      };

      services.AddSingleton(manager);
    }

    public void Init(ITestOutputHelper testOutputHelper)
    {
      _testOutputHelper = testOutputHelper;
    }
    public async Task<T> ValidateSuccessResponse<T>(HttpResponseMessage response)
    {
      var content = await response.Content.ReadAsStringAsync();
      response.EnsureSuccessStatusCode();

      var result = JsonConvert.DeserializeObject<T>(content);
      result.Should().NotBeNull();
      _testOutputHelper.WriteLine($"Success Result:\n{content}");
      return result;
    }
    public HttpContent GetHttpContent(dynamic model)
    {
      return new StringContent(
        JsonConvert.SerializeObject(model),
        Encoding.UTF8,
        "application/json");
    }

    public async Task<dynamic> ExecuteGraphQlQuery(string query, string operationName)
    {
      var graphQlRequest = new GraphQLRequest(query, null, operationName);
      var graphQlResponse = await GqlClient.SendQueryAsync<dynamic>(graphQlRequest, CancellationToken.None);

      if (!(graphQlResponse.Errors?.Length > 0))
        return graphQlResponse.Data;

      foreach (var err in graphQlResponse.Errors)
        _testOutputHelper.WriteLine("ERROR: " + JsonConvert.SerializeObject(err));
      throw new InvalidOperationException(graphQlResponse.Errors.First().Message);
    }

    public void Dispose()
    {
      Client.Dispose();
      _testServer.Dispose();
    }
  }
}