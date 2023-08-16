using System.Collections.Generic;
using System.Threading.Tasks;
using MockApiServer.Models;

namespace MockApiServer.Services
{
  public interface IMockDataService
  {
    Task<(string, TestCase?)> ReadFile(string httpMethod, string url, string queryString=null, dynamic razorModel=null);
    string GetHomeScreen();
    Task WriteFile(TestCase testCase);
    Task WriteFile(GraphQlTestCase testCase);
    Task<IEnumerable<string>> GetPersistedFileNames();
    Task DeleteFile(string method, string path, string queryString=null);
    Task DeleteFile(string fileName);
    void SetupExpectation(TestCase testCase);
    void SetupExpectation(GraphQlTestCase testCase);
    int Expect(string method, in int count, string path, string queryString=null);
    int Expect(GraphQlTestCase testCase, in int count);
    IEnumerable<string> GetExpectationKeys();
    Task<string> ReadGraphQlFile(GraphQlRequest graphQlRequest);
  }
}