using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MockApiServer.Models;
using Newtonsoft.Json;

namespace MockApiServer.Services
{
  public class MockDataService : IMockDataService
  {
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public MockDataService(IConfiguration configuration, IWebHostEnvironment env)
    {
      _configuration = configuration;
      _env = env;
      _validate();
    }

    public async Task<string> ReadFile(string httpMethod, string url)
    {
      var fileName = _getFileNameFromUrl(httpMethod, url);
      var filePath = _getFilePath(fileName);
      if (!File.Exists(filePath))
        throw new FileNotFoundException($"Mock data not found: {fileName}", fileName);
      return await File.ReadAllTextAsync(filePath);
    }
    public string GetHomeScreen()
    {
      return File.ReadAllText(string.IsNullOrEmpty(_env.WebRootPath) ?
        Path.Combine(Assembly.GetExecutingAssembly().Location, "index.html") :
        Path.Combine(_env.WebRootPath, "index.html"));
    }

    public async Task WriteFile(ExpectedTestResult expectedResult)
    {
      var fileName = _getFileNameFromUrl(expectedResult.HttpMethod, expectedResult.RequestPath);

      await File.WriteAllTextAsync(_getFilePath(fileName), JsonConvert.SerializeObject(expectedResult.ExpectedResult), CancellationToken.None);
    }

    public Task<IEnumerable<string>> GetPersistedFileNames()
    {
      var workingDirectory = new DirectoryInfo(_getWorkingDirectory());
      return Task.FromResult(workingDirectory
        .GetFiles("*.json")
        .Select(s => s.Name));
    }

    public Task DeleteFile(string method, string path)
    {
      var filePath = _getFilePath(_getFileNameFromUrl(method, path));
      if (!File.Exists(filePath))
        throw new FileNotFoundException();

      File.Delete(filePath);
      return Task.CompletedTask;
    }

    private void _validate()
    {
      var workingFolder = _getWorkingDirectory();

      if (string.IsNullOrEmpty(workingFolder))
        throw new InvalidOperationException("No data directory specified. Please specify Data Directory");

      var directory = new DirectoryInfo(workingFolder);
      if (!directory.Exists)
        throw new DirectoryNotFoundException($"Could not find directory specified: {directory.FullName}");
    }
    private string _getFilePath(string fileName)
    {
      return Path.Combine(_getWorkingDirectory(), fileName);
    }
    private string _getWorkingDirectory()
    {
      return Path.Combine(Environment.CurrentDirectory, _env.WebRootPath, "mockData");
    }
    private string _getFileNameFromUrl(string httpMethod, string url)
    {
      if (url.StartsWith("/"))
        url = url.Substring(1);
      return $"{httpMethod.ToLower()}_{url.ToLower().Replace('/', '_')}.json";
    }
  }

  public interface IMockDataService
  {
    Task<string> ReadFile(string httpMethod, string url);
    string GetHomeScreen();
    Task WriteFile(ExpectedTestResult expectedResult);
    Task<IEnumerable<string>> GetPersistedFileNames();
    Task DeleteFile(string method, string path);
  }
}
