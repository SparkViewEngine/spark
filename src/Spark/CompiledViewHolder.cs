// Copyright 2008-2024 Louis DeJardin
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

namespace Spark
{
    public interface ICompiledViewHolder 
    {
        ISparkViewEntry Lookup(SparkViewDescriptor descriptor);
        ISparkViewEntry Lookup(Guid viewId);
        void Store(ISparkViewEntry entry);
    }

    public class CompiledViewHolder : ICompiledViewHolder
    {
        readonly Dictionary<SparkViewDescriptor, ISparkViewEntry> _cache = new Dictionary<SparkViewDescriptor, ISparkViewEntry>();

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

