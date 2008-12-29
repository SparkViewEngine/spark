using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml.XPath;
using Spark;
using Spark.Parser;
using SparkLanguage.VsAdapters;
using SparkLanguagePackageLib;

namespace SparkLanguage
{
    public class VsProjectSparkSettings : ISparkSettings
    {
        readonly HierarchyItem _root;
        private string _defaultBaseType;

        private XPathNavigator _sparkSection;
        private readonly Dictionary<string, DateTime> _dependencies = new Dictionary<string, DateTime>();

        public VsProjectSparkSettings(IVsHierarchy hierarchy)
        {
            _root = new HierarchyItem(hierarchy, 0xfffffffe);
        }

        XPathNavigator GetWebConfig()
        {
            // check if the loaded sources have changed
            if (_dependencies.Any(kv => File.GetLastWriteTimeUtc(kv.Key) != kv.Value))
                _sparkSection = null;

            // return loaded source if valid
            if (_sparkSection != null)
                return _sparkSection;

            _dependencies.Clear();

            // locate and load web.config
            var config = _root.FindPath("web.config");
            if (config == null) return null;

            _dependencies.Add(config.CanonicalName, File.GetLastWriteTimeUtc(config.CanonicalName));
            var doc = new XPathDocument(config.CanonicalName);
            var navigator = doc.CreateNavigator();
            _sparkSection = navigator.SelectSingleNode("configuration/spark");
            if (_sparkSection == null) return null;

            // check for external config source attribute
            var configSource = _sparkSection.SelectSingleNode("@configSource");
            if (configSource != null)
            {
                _sparkSection = null;

                // locate and load that source instead
                config = _root.FindPath(configSource.Value);
                if (config == null) return null;

                _dependencies.Add(config.CanonicalName, File.GetLastWriteTimeUtc(config.CanonicalName));
                doc = new XPathDocument(config.CanonicalName);
                navigator = doc.CreateNavigator();
                _sparkSection = navigator.SelectSingleNode("spark");
                if (_sparkSection == null) return null;
            }

            return _sparkSection;
        }

        T GetSetting<T>(string xpath, T defaultValue)
        {
            var spark = GetWebConfig();
            if (spark == null)
                return defaultValue;

            var node = spark.SelectSingleNode(xpath);
            if (node == null)
                return defaultValue;

            return (T)node.ValueAs(typeof(T));
        }

        public bool AutomaticEncoding
        {
            get { return GetSetting("pages/@automaticEncoding", ParserSettings.DefaultAutomaticEncoding); }
        }

        public bool Debug
        {
            get { return false; }
        }

        public NullBehaviour NullBehaviour
        {
            get { return (NullBehaviour)Enum.Parse(typeof(NullBehaviour), GetSetting("compilation/@nullBehavior", "Lenient")); }
        }

        public string Prefix
        {
            get { return GetSetting("pages/@prefix", ""); }
        }


        public string PageBaseType
        {
            get { return GetSetting("pages/@pageBaseType", _defaultBaseType); }
            set { _defaultBaseType = value; }
        }

        public IEnumerable<string> UseNamespaces
        {
            get
            {
                var spark = GetWebConfig();
                if (spark == null)
                    return new string[0];
                return spark.Select("pages/namespaces/add/@namespace")
                    .Cast<XPathNavigator>()
                    .Select(n => n.Value);
            }
        }

        public IEnumerable<string> UseAssemblies
        {
            get
            {
                var spark = GetWebConfig();
                if (spark == null)
                    return new string[0];
                return spark.Select("compilation/assemblies/add/@assembly")
                    .Cast<XPathNavigator>()
                    .Select(n => n.Value);
            }
        }

        public IEnumerable<ResourceMapping> ResourceMappings
        {
            get { return new ResourceMapping[0]; }
        }

        public IEnumerable<IViewFolderSettings> ViewFolders
        {
            get { return new IViewFolderSettings[0]; }
        }
    }
}
