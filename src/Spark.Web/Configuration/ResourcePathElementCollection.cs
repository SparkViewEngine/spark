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

        public void Add(string match, string location, bool stop)
        {
            base.BaseAdd(new ResourcePathElement { Match = match, Location = location, Stop = stop});
        }
    }
}
