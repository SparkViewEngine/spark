// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System.Configuration;
using Spark.Parser;

namespace Spark.Configuration
{
    public class PagesElement : ConfigurationElement
    {
        [ConfigurationProperty("pageBaseType")]
        public string PageBaseType
        {
            get => (string)this["pageBaseType"];
            set => this["pageBaseType"] = value;
        }

        [ConfigurationProperty("prefix")]
        public string Prefix
        {
            get => (string)this["prefix"];
            set => this["prefix"] = value;
        }

        [ConfigurationProperty("automaticEncoding", DefaultValue = ParserSettings.DefaultAutomaticEncoding)]
        public bool AutomaticEncoding
        {
            get => (bool)this["automaticEncoding"];
            set => this["automaticEncoding"] = value;
        }

        [ConfigurationProperty("statementMarker", DefaultValue = "#")]
        public string StatementMarker
        {
            get => (string)this["statementMarker"];
            set => this["statementMarker"] = value;
        }

        [ConfigurationProperty("namespaces")]
        [ConfigurationCollection(typeof(NamespaceElementCollection))]
        public NamespaceElementCollection Namespaces
        {
            get => (NamespaceElementCollection)this["namespaces"];
            set => this["namespaces"] = value;
        }

        [ConfigurationProperty("resources")]
        [ConfigurationCollection(typeof(ResourcePathElementCollection))]
        public ResourcePathElementCollection Resources
        {
            get => (ResourcePathElementCollection)this["resources"];
            set => this["resources"] = value;
        }

        [ConfigurationProperty("parseSectionTagAsSegment")]
        public bool ParseSectionTagAsSegment
        {
            get => (bool)this["parseSectionTagAsSegment"];
            set => this["parseSectionTagAsSegment"] = value;
        }
    }
}
