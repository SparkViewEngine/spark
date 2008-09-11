using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Spark
{
    public class SparkSettings : ISparkSettings
    {
        public SparkSettings()
        {
            UseNamespaces = new List<string>();
            UseAssemblies = new List<string>();
            ResourceMappings = new List<ResourceMapping>();
        }

        public bool Debug { get; set; }
        public string Prefix { get; set; }
        public string PageBaseType { get; set; }

        public IList<string> UseNamespaces { get; set; }
        public IList<string> UseAssemblies { get; set; }
        public IList<ResourceMapping> ResourceMappings { get; set; }

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
            UseAssemblies.Add(assembly);
            return this;
        }

        public SparkSettings AddAssembly(Assembly assembly)
        {
            UseAssemblies.Add(assembly.FullName);
            return this;
        }

        public SparkSettings AddNamespace(string ns)
        {
            UseNamespaces.Add(ns);
            return this;
        }

        public SparkSettings SetPrefix(string prefix)
        {
            Prefix = prefix;
            return this;
        }

        public SparkSettings AddResourceMapping(string match, string replace)
        {
            ResourceMappings.Add(new ResourceMapping { Match = match, Location = replace });
            return this;
        }
    }
}
