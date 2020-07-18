using System;
using System.Collections.Generic;
using System.Dynamic;
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
using RazorLight;
using RazorLight.Razor;

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

    public async Task<string> ReadFile(string httpMethod, string url, string queryString = null, dynamic razorModel=null)
    {
      var filePath = _resolveFileNameFromRequest(httpMethod, url, queryString);

      if (Path.GetExtension(filePath) == ".razor")
        return await _razorResult(filePath, razorModel);

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
      var filePath = _getFilePath(fileName);

      if (expectedResult.IsRazorFile)
        filePath = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(fileName)}.razor");

      await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(expectedResult.ExpectedResult), CancellationToken.None);
    }

    public Task<IEnumerable<string>> GetPersistedFileNames()
    {
      var workingDirectory = new DirectoryInfo(_getWorkingDirectory());
      return Task.FromResult(workingDirectory
        .GetFiles("*.json", SearchOption.TopDirectoryOnly)
        .Union(workingDirectory.GetFiles("*.razor", SearchOption.TopDirectoryOnly))
        .Select(s => s.Name));
    }

    public Task DeleteFile(string method, string path, string queryString = null)
    {
      var filePath = _resolveFileNameFromRequest(method, path, queryString);
      File.Delete(filePath);
      return Task.CompletedTask;
    }

    #region Private
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
          "_" + BitConverter
            .ToString(_md5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(queryString)))
            .Replace("-", string.Empty)
        : null;

      return $"{httpMethod.ToLower()}_{url.ToLower().Replace('/', '_')}{queryStringHash}.json";
    }
    private async Task<string> _razorResult(string filePath, dynamic razorModel)
    {
      var engine = new RazorLightEngineBuilder()
        .UseProject(new EmbeddedRazorProject(Assembly.GetExecutingAssembly()))
        .UseMemoryCachingProvider()
        .Build();

      var templateKey = Path.GetFileNameWithoutExtension(filePath);
      var template = File.ReadAllText(filePath);

      if (razorModel == null)
      {
        razorModel = new ExpandoObject();
      }
      razorModel.TemplateName = templateKey;

      return await engine.CompileRenderStringAsync(templateKey, template, razorModel, (ExpandoObject)null);
    }
    private string _resolveFileNameFromRequest(string httpMethod, string url, string queryString)
    {
      var fileName = _getFileNameFromUrl(httpMethod, url, queryString);
      _getFilePath(fileName);

      var workingDirectory = new DirectoryInfo(_getWorkingDirectory());
      var files = workingDirectory.GetFiles($"{Path.GetFileNameWithoutExtension(fileName)}.*");
      if (files.Length == 0)
        throw new FileNotFoundException($"Mock data not found: {fileName}", fileName);
      if (files.Length > 1)
        throw new InvalidOperationException($"More than one result found: {string.Join(',', files.Select(x => x.Name))}");
      return files.First().FullName;
    }
    #endregion
  }
}
