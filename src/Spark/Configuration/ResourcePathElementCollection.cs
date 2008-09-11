using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Spark.Configuration
{
    public class ResourcePathElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ResourcePathElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ResourcePathElement)element).Match;
        }

        public void Add(string match, string location)
        {
            base.BaseAdd(new ResourcePathElement { Match = match, Location = location });
        }
    }
}
