using System.Collections.Generic;

namespace Spark
{
    public interface IPartialProvider
    {
        IEnumerable<string> GetPaths(string viewPath);
    }
}