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
using System;
using System.Linq;
using System.Collections.Generic;
using Spark;

namespace Spark
{
    public class CompiledViewHolder
    {
        private CompiledViewHolder _current;

        readonly Dictionary<SparkViewDescriptor, ISparkViewEntry> _cache = new Dictionary<SparkViewDescriptor, ISparkViewEntry>();

        public CompiledViewHolder Current
        {
            get
            {
                if (_current == null)
                    _current = new CompiledViewHolder();
                return _current;
            }
            set { _current = value; }
        }

        public ISparkViewEntry Lookup(SparkViewDescriptor descriptor)
        {
            ISparkViewEntry entry;

            lock (_cache)
            {
                if (!_cache.TryGetValue(descriptor, out entry))
                    return null;
            }

            return entry.IsCurrent() ? entry : null;
        }

        public ISparkViewEntry Lookup(Guid viewId)
        {
            lock (_cache)
            {
                return _cache.Values.FirstOrDefault(e => e.ViewId == viewId);
            }
        }

        public void Store(ISparkViewEntry entry)
        {
            lock (_cache)
            {
                _cache[entry.Descriptor] = entry;
            }
        }
    }
}

