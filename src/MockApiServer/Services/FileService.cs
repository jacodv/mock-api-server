using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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
    private readonly MD5CryptoServiceProvider _md5CryptoServiceProvider;

    public MockDataService(IConfiguration configuration, IWebHostEnvironment env)
    {
      _configuration = configuration;
      _env = env;
      _validate();
      _md5CryptoServiceProvider = new MD5CryptoServiceProvider();
    }

    public async Task<string> ReadFile(string httpMethod, string url, string queryString = null)
    {
      var fileName = _getFileNameFromUrl(httpMethod, url, queryString);
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
      var fileName = _getFileNameFromUrl(expectedResult.HttpMethod, expectedResult.RequestPath, expectedResult.QueryString);
      await File.WriteAllTextAsync(_getFilePath(fileName), JsonConvert.SerializeObject(expectedResult.ExpectedResult), CancellationToken.None);
    }

    public Task<IEnumerable<string>> GetPersistedFileNames()
    {
      var workingDirectory = new DirectoryInfo(_getWorkingDirectory());
      return Task.FromResult(workingDirectory
        .GetFiles("*.json")
        .Select(s => s.Name));
    }

    public Task DeleteFile(string method, string path, string queryString=null)
    {
      var filePath = _getFilePath(_getFileNameFromUrl(method, path, queryString));
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
    private string _getFileNameFromUrl(string httpMethod, string url, string queryString)
    {
      if (url.StartsWith("/"))
        url = url.Substring(1);
      
      if (!string.IsNullOrEmpty(queryString) && queryString.StartsWith("?"))
        queryString = queryString.Substring(1);
      
      var queryStringHash = !string.IsNullOrEmpty(queryString) ?
          "_"+BitConverter
            .ToString(_md5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(queryString)))
            .Replace("-", string.Empty)
        : null;

      return $"{httpMethod.ToLower()}_{url.ToLower().Replace('/', '_')}{queryStringHash}.json";
    }
  }

  public interface IMockDataService
  {
    Task<string> ReadFile(string httpMethod, string url, string queryString=null);
    string GetHomeScreen();
    Task WriteFile(ExpectedTestResult expectedResult);
    Task<IEnumerable<string>> GetPersistedFileNames();
    Task DeleteFile(string method, string path, string queryString=null);
  }
}
