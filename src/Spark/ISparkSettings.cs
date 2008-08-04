using System.Collections.Generic;

namespace Spark
{
    public interface ISparkSettings
    {
        bool Debug { get; }
        IList<string> UseNamespaces { get; }
        IList<string> UseAssemblies { get; }
    }
}