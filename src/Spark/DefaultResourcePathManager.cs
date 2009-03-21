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
            var effectivePath = path;
            foreach(var mapping in _settings.ResourceMappings)
            {
                if (!mapping.IsMatch(effectivePath)) 
                    continue;

                effectivePath = mapping.Map(effectivePath);
                if (mapping.Stop)
                    return effectivePath;
            }
            if (effectivePath.StartsWith("~/", StringComparison.InvariantCultureIgnoreCase))
            {
                effectivePath = effectivePath.Substring(1);
            }
            return PathConcat(siteRoot, effectivePath);
        }

        public string PathConcat(string siteRoot, string path)
        {
            var trailingSlash = siteRoot.EndsWith("/");
            var leadingSlash = path.StartsWith("/");
            
            if (leadingSlash && trailingSlash)
                return siteRoot + path.Substring(1);
            
            if (leadingSlash || trailingSlash)
                return siteRoot + path;

            return siteRoot + "/" + path;
        }

    }
}
