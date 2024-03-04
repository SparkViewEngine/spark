namespace Spark.AspNetCore.Mvc.Helpers;

public static class PathHelper
{
    public static string GetAbsolutePath(string executingFilePath, string pagePath)
    {
        // Path is not valid or a page name; no change required.
        if (string.IsNullOrEmpty(pagePath) || !IsRelativePath(pagePath))
        {
            return pagePath;
        }

        if (IsAbsolutePath(pagePath))
        {
            // An absolute path already; no change required.
            return pagePath.Replace("~/", string.Empty);
        }

        // Given a relative path i.e. not yet application-relative (starting with "~/" or "/"), interpret
        // path relative to currently-executing view, if any.
        if (string.IsNullOrEmpty(executingFilePath))
        {
            // Not yet executing a view. Start in app root.
            return $"/{pagePath}";
        }

        // Get directory name (including final slash) but do not use Path.GetDirectoryName() to preserve path
        // normalization.
        var index = executingFilePath.LastIndexOf('/');
        return executingFilePath.Substring(0, index + 1) + pagePath;
    }

    public static bool IsAbsolutePath(string name) => name.StartsWith("~/") || name.StartsWith("/");

    // Though ./ViewName looks like a relative path, framework searches for that view using view locations.
    public static bool IsRelativePath(string name) => !IsAbsolutePath(name);
}