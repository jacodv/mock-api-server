using System;
using FluentValidation;

namespace MockApiServer.Models
{
  public class TestCase
  {
    public string HttpMethod { get; set; }
    public string RequestPath { get; set; }
    public dynamic ExpectedResult { get; set; }
    public string QueryString { get; set; }
    public bool IsRazorFile { get; set; }
  }

  public class GraphQlTestCase
  {
    public string Query { get; set; }
    public string OperationName { get; set; }
    public dynamic ExpectedResult { get; set; }
  }

  public class TestCaseValidator : AbstractValidator<TestCase>
  {
    public TestCaseValidator()
    {
      RuleFor(x => x.HttpMethod).NotEmpty();
      RuleFor(x => x.RequestPath).NotEmpty();
      RuleFor(x => x.ExpectedResult).NotNull();
    }
  }

  public class GraphQlTestCaseValidator : AbstractValidator<GraphQlTestCase>
  {
    public GraphQlTestCaseValidator()
    {
      RuleFor(x => x.Query).NotEmpty();
      RuleFor(x => x.OperationName).NotEmpty();
      RuleFor(x => x.ExpectedResult).NotNull();
    }
  }
}