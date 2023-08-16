using System;
using System.Collections.Generic;
using FluentValidation;

namespace MockApiServer.Models
{
  public class TestCase
  {
    public string HttpMethod { get; set; }
    public string RequestPath { get; set; }
    public dynamic ExpectedResult { get; set; }
    public string? QueryString { get; set; }
    public bool IsRazorFile { get; set; }
    public bool IsStaticContent { get; set; }
    public bool IsBinary { get; set; }
    public string? StaticContentExtension { get; set; }
    public Dictionary<string, string?> RequiredHeaders = new();

    public TestCase(string httpMethod, string requestPath, dynamic expectedResult)
    {
      HttpMethod = httpMethod;
      RequestPath = requestPath;
      ExpectedResult= expectedResult;
    }
  }

  public class GraphQlTestCase
  {
    public GraphQlTestCase(string query, string operationName, dynamic expectedResult)
    {
      Query = query;
      OperationName = operationName;
      ExpectedResult = expectedResult;
    }

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
      RuleFor(x => x).Custom((staticTestCase, context) =>
      {
        if (!staticTestCase.IsStaticContent)
          return;

        if (string.IsNullOrEmpty(staticTestCase.StaticContentExtension))
        {
          context.AddFailure(nameof(staticTestCase.StaticContentExtension), "No extension provided for the static content");
          return;
        }

        if (staticTestCase.IsBinary)
        {
          try
          {
            var binaryData = Convert.FromBase64String((string)staticTestCase.ExpectedResult);
          }
          catch (Exception e)
          {
            Console.WriteLine($"{e.Message}\n{e}");
            context.AddFailure(nameof(staticTestCase.ExpectedResult), "Failed to convert from the Base64 string");
          }
        }
      });
    }

    //private bool _validateStaticContent(TestCase staticTestCase)
    //{
    //  if (string.IsNullOrEmpty(staticTestCase.StaticContentExtension))
    //}
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