using System;
using System.Collections.Generic;

namespace Spark
{
    public interface ISparkSettings
    {
        bool Debug { get; }
        string PageBaseType { get; set; }
        IList<string> UseNamespaces { get; }
        IList<string> UseAssemblies { get; }
    }
}

