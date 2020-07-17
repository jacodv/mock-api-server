using FluentValidation;

namespace MockApiServer.Models
{
  public class ExpectedTestResult
  {
    public string HttpMethod { get; set; }
    public string RequestPath { get; set; }
    public dynamic ExpectedResult { get; set; }
  }

  public class ExpectedTestResultValidator : AbstractValidator<ExpectedTestResult>
  {
    public ExpectedTestResultValidator()
    {
      RuleFor(x => x.HttpMethod).NotEmpty();
      RuleFor(x => x.RequestPath).NotEmpty();
      RuleFor(x => x.ExpectedResult).NotNull();
    }
  }
}