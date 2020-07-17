namespace MockApiServer.Helpers
{
  public static class StringHelpers
  {
    public static string Remove(this string source, string original, string replace = "")
    {
      return source.StartsWith(original) ? 
        source.Replace(original, replace) : source;
    }

    public static string RemoveTrailing(this string source, char entity)
    {
      return source.EndsWith(entity) ? 
        source.Remove(source.Length - 1) : source;
    }

    public static string RemoveLeading(this string source, char entity)
    {
      return source.StartsWith(entity) ? source.Remove(0, 1) : source;
    }

    public static string ReplaceDoubleSlashes(this string source)
    {
      return source.Replace("//", "/");
    }
  }
}
