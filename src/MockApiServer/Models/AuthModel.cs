using System;

namespace MockApiServer.Models
{
  public class AuthModel
  {
    public string UserName { get; set; }
    public string Token { get; set; }
    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }
    public int ExpiresInSeconds { get; set; }
  }
}
