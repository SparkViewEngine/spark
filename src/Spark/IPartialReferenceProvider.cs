using System.Collections.Generic;

namespace Spark
{
    public interface IPartialReferenceProvider
    {
        IEnumerable<string> GetPaths(string viewPath, bool allowCustomReferencePath);
    }
}