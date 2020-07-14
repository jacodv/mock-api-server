using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MockApiJsonServer.Services
{
  public class FileService : IFileService
  {
    public FileService(IConfiguration configuration)
    {
      LoadFiles(configuration);
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

    public string GetHomeScreen(string webRootPath)
    {
      return File.ReadAllText(Path.Combine(webRootPath, "index.html"));
    }

    private void LoadFiles(IConfiguration configuration)
    {
      var workingFolder = configuration.GetValue<string>("WorkingDirectory");

      if (string.IsNullOrEmpty(workingFolder))
        workingFolder = Path.Combine(Assembly.GetExecutingAssembly().Location, "Data");

      var directory = new DirectoryInfo(workingFolder);
      if(!directory.Exists)
        directory.Create();

      Files = directory.GetFiles();
    }
  }

  public interface IFileService
  {
    object ReadFile(string fileName);
    string GetHomeScreen(string webRootPath);
  }
}
