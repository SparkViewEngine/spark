using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Spark.FileSystem;

namespace Spark
{
    public class SparkSettings : ISparkSettings
    {
        public SparkSettings()
        {
            _useNamespaces = new List<string>();
            _useAssemblies = new List<string>();
            _resourceMappings = new List<ResourceMapping>();
            _viewFolders = new List<IViewFolderSettings>();
        }

        public bool Debug { get; set; }
        public string Prefix { get; set; }
        public string PageBaseType { get; set; }

        private readonly IList<string> _useNamespaces;
        public IEnumerable<string> UseNamespaces
        {
            get { return _useNamespaces; }
        }

        private readonly IList<string> _useAssemblies;
        public IEnumerable<string> UseAssemblies
        {
            get { return _useAssemblies; }
        }

        private readonly IList<ResourceMapping> _resourceMappings;
        public IEnumerable<ResourceMapping> ResourceMappings
        {
            get { return _resourceMappings; }
        }

        private readonly IList<IViewFolderSettings> _viewFolders;
        public IEnumerable<IViewFolderSettings> ViewFolders
        {
            get { return _viewFolders; }
        }

        public SparkSettings SetDebug(bool debug)
        {
            Debug = debug;
            return this;
        }

        public SparkSettings SetPageBaseType(string typeName)
        {
            PageBaseType = typeName;
            return this;
        }

        public SparkSettings SetPageBaseType(Type type)
        {
            PageBaseType = type.FullName;
            return this;
        }

        public SparkSettings AddAssembly(string assembly)
        {
            _useAssemblies.Add(assembly);
            return this;
        }

        public SparkSettings AddAssembly(Assembly assembly)
        {
            _useAssemblies.Add(assembly.FullName);
            return this;
        }

        public SparkSettings AddNamespace(string ns)
        {
            _useNamespaces.Add(ns);
            return this;
        }

        public SparkSettings SetPrefix(string prefix)
        {
            Prefix = prefix;
            return this;
        }

        public SparkSettings AddResourceMapping(string match, string replace)
        {
            _resourceMappings.Add(new ResourceMapping { Match = match, Location = replace });
            return this;
        }

        public SparkSettings AddViewFolder(ViewFolderType type, IDictionary<string, string> parameters)
        {
            _viewFolders.Add(new ViewFolderSettings
                                 {
                                     FolderType = type,
                                     Parameters = parameters
                                 });
            return this;
        }

        public SparkSettings AddViewFolder(Type customType, IDictionary<string, string> parameters)
        {
            _viewFolders.Add(new ViewFolderSettings
                                 {
                                     FolderType = ViewFolderType.Custom,
                                     Type = customType.AssemblyQualifiedName,
                                     Parameters = parameters
                                 });
            return this;
        }
    }
}
