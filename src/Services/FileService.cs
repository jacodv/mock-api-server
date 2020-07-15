using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MockApiServer.Services
{
  public class FileService : IFileService
  {
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public FileService(IConfiguration configuration, IWebHostEnvironment env)
    {
      _configuration = configuration;
      _env = env;
      LoadFiles();
    }
    private FileInfo[] Files { get; set; }

    public object ReadFile(string fileName)
    {
      var file = Files.FirstOrDefault(x => x.Name == fileName);
      if (file == null)
        return null;
      var json = File.ReadAllText(file.FullName);
      return JsonConvert.DeserializeObject(json);
    }

    public string GetHomeScreen()
    {
      return File.ReadAllText(string.IsNullOrEmpty(_env.WebRootPath) ? 
        Path.Combine(Assembly.GetExecutingAssembly().Location, "index.html") : 
        Path.Combine(_env.WebRootPath, "index.html"));
    }

    private void LoadFiles()
    {
      var workingFolder = _configuration.GetValue<string>("DataFolder") ?? _configuration.GetValue<string>("WorkingDirectory");

      if (string.IsNullOrEmpty(workingFolder))
        throw new InvalidOperationException("No data directory specified. Please specify Data Directory");

      var directory = new DirectoryInfo(workingFolder);
      if(!directory.Exists)
        throw new DirectoryNotFoundException($"Could not find directory specified: {directory.FullName}");
      Files = directory.GetFiles();
    }
  }

  public interface IFileService
  {
    object ReadFile(string fileName);
    string GetHomeScreen();
  }
}
