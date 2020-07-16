using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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

    public string ReadFile(string httpMethod, string url)
    {
      var fileName = $"{httpMethod}_{_getFileNameFromUrl(url)}";
      var filePath = _getFilePath(fileName);
      if(!File.Exists(filePath))
        throw new FileNotFoundException($"Mock data not found: {fileName}", fileName);
      return File.ReadAllText(filePath);
    }
    public string GetHomeScreen()
    {
      return File.ReadAllText(string.IsNullOrEmpty(_env.WebRootPath) ? 
        Path.Combine(Assembly.GetExecutingAssembly().Location, "index.html") : 
        Path.Combine(_env.WebRootPath, "index.html"));
    }

    private void _validate()
    {
      var workingFolder = _getWorkingDirectory();

      if (string.IsNullOrEmpty(workingFolder))
        throw new InvalidOperationException("No data directory specified. Please specify Data Directory");

      var directory = new DirectoryInfo(workingFolder);
      if(!directory.Exists)
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
    private string _getFileNameFromUrl(string url)
    {
      if (url.StartsWith("/"))
        url = url.Substring(1);
      return $"{url.Replace('/', '_')}.json";
    }
  }

  public interface IMockDataService
  {
    string ReadFile(string httpMethod, string url);
    string GetHomeScreen();
  }
}
