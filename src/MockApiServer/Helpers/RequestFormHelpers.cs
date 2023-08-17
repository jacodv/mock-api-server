using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace MockApiServer.Helpers
{
  public static class RequestFormHelpers
  {
    public static IDictionary<string, string> ToDictionary(this IFormCollection form)
    {
      return form
        .Keys
        .ToDictionary<string, string, string>(
          key => key, 
          key => form[key]!);
    }
  }
}
