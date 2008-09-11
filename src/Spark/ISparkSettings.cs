using System;
using System.Collections.Generic;

namespace Spark
{
    public interface ISparkSettings
    {
        bool Debug { get; }
        string Prefix { get; }
        string PageBaseType { get; set; }
        IList<string> UseNamespaces { get; }
        IList<string> UseAssemblies { get; }
        IList<ResourceMapping> ResourceMappings { get; }
    }

    public class ResourceMapping
    {
        public string Match { get; set; }
        public string Location { get; set; }
    }
}

