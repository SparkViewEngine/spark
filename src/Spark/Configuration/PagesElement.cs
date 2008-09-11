using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Spark.Configuration
{
    public class PagesElement : ConfigurationElement
    {
        [ConfigurationProperty("pageBaseType")]
        public string PageBaseType
        {
            get { return (string)this["pageBaseType"]; }
            set { this["pageBaseType"] = value; }
        }

        [ConfigurationProperty("prefix")]
        public string Prefix
        {
            get { return (string)this["prefix"]; }
            set { this["prefix"] = value; }
        }

        [ConfigurationProperty("namespaces")]
        [ConfigurationCollection(typeof(NamespaceElementCollection))]
        public NamespaceElementCollection Namespaces
        {
            get { return (NamespaceElementCollection)this["namespaces"]; }
            set { this["namespaces"] = value; }
        }

        [ConfigurationProperty("resources")]
        [ConfigurationCollection(typeof(ResourcePathElementCollection))]
        public ResourcePathElementCollection Resources
        {
            get { return (ResourcePathElementCollection)this["resources"]; }
            set { this["resources"] = value; }
        }
    }
}
