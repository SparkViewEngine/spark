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
using System.Collections.Generic;
using System.Configuration;

namespace Spark.Configuration
{
    public class ViewFolderElement : ConfigurationElement, IViewFolderSettings
    {
        public ViewFolderElement()
        {
            Parameters = new Dictionary<string, string>();
        }

        [ConfigurationProperty("name")]
        public string Name
        {
            get => (string)this["name"];
            set => this["name"] = value;
        }
        
        [ConfigurationProperty("type")]
        public string Type
        {
            get => (string)this["type"];
            set => this["type"] = value;
        }

        [ConfigurationProperty("subfolder")]
        public string Subfolder
        {
            get => (string)this["subfolder"];
            set => this["subfolder"] = value;
        }

        public IDictionary<string, string> Parameters { get; set;}
        
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            Parameters.Add(name, value);
            return true;
        }
    }
}