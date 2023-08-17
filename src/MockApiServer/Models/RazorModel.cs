// ReSharper disable InconsistentNaming
namespace MockApiServer.Models
{
  public class RazorModel
  {
    public string? TemplateName { get; set; }
    public string? HttpMethod { get; set; }
    public string? RequestPath { get; set; }
    public string? QueryString { get; set; }
    public string? RequestBody { get; set; }
  }

  public class GraphQlRequest
  {
    public string? query { get; set; }
    public string? variables { get; set; }
    public string? operationName { get; set; }
  }
}
