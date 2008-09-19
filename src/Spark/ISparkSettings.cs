using System;
using System.Collections.Generic;
using Spark.FileSystem;

namespace Spark
{
    public interface ISparkSettings
    {
        bool Debug { get; }
        string Prefix { get; }
        string PageBaseType { get; set; }
        IEnumerable<string> UseNamespaces { get; }
        IEnumerable<string> UseAssemblies { get; }
        IEnumerable<ResourceMapping> ResourceMappings { get; }
        IEnumerable<IViewFolderSettings> ViewFolders { get; }
    }

    public class ResourceMapping
    {
        public string Match { get; set; }
        public string Location { get; set; }
    }

    public interface IViewFolderSettings
    {
        string Name { get; set; }
        ViewFolderType FolderType { get; set; }
        string Type { get; set; }
        string Subfolder { get; set; }
        IDictionary<string, string> Parameters { get; set; }
    }

    internal class ViewFolderSettings : IViewFolderSettings
    {
        public string Name { get; set; }
        public ViewFolderType FolderType { get; set; }
        public string Type { get; set; }
        public string Subfolder { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
    }
}

