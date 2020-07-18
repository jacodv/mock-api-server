using System.Collections.Generic;
using System.Threading.Tasks;
using MockApiServer.Models;

namespace MockApiServer.Services
{
  public interface IMockDataService
  {
    Task<string> ReadFile(string httpMethod, string url, string queryString=null, dynamic razorModel=null);
    string GetHomeScreen();
    Task WriteFile(ExpectedTestResult expectedResult);
    Task<IEnumerable<string>> GetPersistedFileNames();
    Task DeleteFile(string method, string path, string queryString=null);
  }
}