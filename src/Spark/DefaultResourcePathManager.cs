// Copyright 2008 Louis DeJardin - http://whereslou.com
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
using System;

namespace Spark
{
    public class DefaultResourcePathManager : IResourcePathManager
    {
        private readonly ISparkSettings _settings;

        public DefaultResourcePathManager(ISparkSettings settings)
        {
            _settings = settings;
        }

        public string GetResourcePath(string siteRoot, string path)
        {
            foreach(var mapping in _settings.ResourceMappings)
            {
                if (path.StartsWith(mapping.Match, StringComparison.InvariantCultureIgnoreCase))
                {
                    return mapping.Location + path.Substring(mapping.Match.Length);
                }
            }
            return siteRoot + path;
        }
    }
}
