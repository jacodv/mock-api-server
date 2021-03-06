﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
    private readonly Dictionary<string, dynamic> _expectations;
    private readonly Dictionary<string, int> _expectationRead;

    public MockDataService(IConfiguration configuration, IWebHostEnvironment env)
    {
      _configuration = configuration;
      _env = env;
      _validate();
      _md5CryptoServiceProvider = new MD5CryptoServiceProvider();
      _expectations = new Dictionary<string, dynamic>();
      _expectationRead = new Dictionary<string, int>();
    }

    public async Task<string> ReadFile(string httpMethod, string url, string queryString = null, dynamic razorModel=null)
    {
      var fileName = _getFileNameFromUrl(httpMethod, url, queryString);

      var expectation = _getExpectation(fileName);
      if (expectation != null)
        return expectation;
        
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

    public async Task WriteFile(TestCase testCase)
    {
      var fileName = _getFileNameFromUrl(testCase.HttpMethod, testCase.RequestPath, testCase.QueryString);
      var filePath = _getFilePath(fileName);

      var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
      if (testCase.IsRazorFile)
        filePath = Path.Combine(Path.GetDirectoryName(filePath), $"{fileNameWithoutExtension}.razor");
      else if (testCase.IsStaticContent)
      {
        if(!fileName.EndsWith(testCase.StaticContentExtension))
          filePath = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.ChangeExtension(fileNameWithoutExtension, testCase.StaticContentExtension)}");
      }

      await File.WriteAllTextAsync(
        filePath,
        (testCase.IsRazorFile && testCase.ExpectedResult is string)?
          testCase.ExpectedResult: // Razor files will post string and not json
          JsonConvert.SerializeObject(testCase.ExpectedResult), 
        CancellationToken.None);
    }

    public async Task WriteFile(GraphQlTestCase testCase)
    {
      var fileName = _getGraphQlFileName(testCase.OperationName, testCase.Query);
      var filePath = _getFilePath(fileName);

      await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(testCase.ExpectedResult), CancellationToken.None);
    }

    public Task<IEnumerable<string>> GetPersistedFileNames()
    {
      var workingDirectory = new DirectoryInfo(_getWorkingDirectory());
      return Task.FromResult(workingDirectory
        .GetFiles("*.*", SearchOption.TopDirectoryOnly)
        .Select(s => s.Name));
    }

    public Task DeleteFile(string method, string path, string queryString = null)
    {
      var filePath = _resolveFileNameFromRequest(method, path, queryString);
      File.Delete(filePath);
      return Task.CompletedTask;
    }

    public Task DeleteFile(string fileName)
    {
      var filePath = _getFilePath(fileName);
      if(!File.Exists(filePath))
        throw new FileNotFoundException($"Invalid file: {fileName}", fileName);
      File.Delete(filePath);
      return Task.CompletedTask;
    }

    public void SetupExpectation(TestCase testCase)
    {
      var fileName = _getFileNameFromUrl(testCase.HttpMethod, testCase.RequestPath, testCase.QueryString);
      _setupExpectation(testCase, fileName);
    }
    public void SetupExpectation(GraphQlTestCase testCase)
    {
      var fileName = _getGraphQlFileName(testCase.OperationName, testCase.Query);
      _setupExpectation(testCase, fileName);
    }

    public int Expect(string method, in int count, string path, string queryString = null)
    {
      var fileName = _getFileNameFromUrl(method, path, queryString);
      return _expect(fileName);
    }

    public int Expect(GraphQlTestCase testCase, in int count)
    {
      var fileName = _getGraphQlFileName(testCase.OperationName, testCase.Query);
      return _expect(fileName);
    }

    public IEnumerable<string> GetExpectationKeys()
    {
      return _expectations.Keys;
    }

    public async Task<string> ReadGraphQlFile(GraphQlRequest graphQlRequest)
    {
      var fileName = _getGraphQlFileName(graphQlRequest.operationName, graphQlRequest.query);
      var expectation = _getExpectation(fileName);
      if (expectation != null)
        return expectation;

      var filePath = _getFilePath(fileName);
      return await File.ReadAllTextAsync(filePath);
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
    private async Task<string> _razorResult(string filePath, RazorModel razorModel)
    {
      var engine = new RazorLightEngineBuilder()
        .UseProject(new EmbeddedRazorProject(Assembly.GetExecutingAssembly()))
        .UseMemoryCachingProvider()
        .Build();

      var templateKey = Path.GetFileNameWithoutExtension(filePath);
      var template = File.ReadAllText(filePath);

      if(razorModel==null)
        razorModel =new RazorModel();
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
    private string _getExpectation(string fileName)
    {
      if (!_expectations.ContainsKey(fileName))
        return null;
      _expectationRead[fileName] = _expectationRead[fileName]+1;
      return JsonConvert.SerializeObject(_expectations[fileName].ExpectedResult);
    }
    private string _getGraphQlFileName(string operationName, string query)
    {
      var minifiedQuery = _getMinifiedJsonString(query);

      var queryHash = BitConverter
        .ToString(_md5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(minifiedQuery)))
        .Replace("-", string.Empty);
      return $"{operationName.ToLower()}_{queryHash}.graphql";
    }
    private int _expect(string fileName)
    {
      if (!_expectations.ContainsKey(fileName))
        return 0;
      var actual = _expectationRead[fileName];
      _expectations.Remove(fileName);
      _expectationRead.Remove(fileName);
      return actual;
    }
    private string _getMinifiedJsonString(string jsonString)
    {
      return Regex.Replace(jsonString, @"\s+", "");
    }
    private void _setupExpectation(dynamic testCase, string fileName)
    {
      if (_expectations.ContainsKey(fileName))
      {
        _expectations[fileName] = testCase;
        return;
      }

      _expectations.Add(fileName, testCase);
      _expectationRead.Add(fileName, 0);
    }
    #endregion
  }
}
