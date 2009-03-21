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
using Spark.Compiler;
using Spark.Parser;
using Spark;

namespace Spark
{
    public class CompiledViewHolder
    {
        static private CompiledViewHolder _current;

        readonly Dictionary<Key, Entry> _cache = new Dictionary<Key, Entry>();

        public static CompiledViewHolder Current
        {
            get
            {
                if (_current == null)
                    _current = new CompiledViewHolder();
                return _current;
            }
            set { _current = value; }
        }

        public Entry Lookup(Key key)
        {
            Entry entry;

            lock (_cache)
            {
                if (!_cache.TryGetValue(key, out entry))
                    return null;
            }

            return entry.Loader.IsCurrent() ? entry : null;
        }

        public Entry Lookup(Guid viewId)
        {
            lock (_cache)
            {
                return _cache.Values.FirstOrDefault(e => e.Compiler.GeneratedViewId == viewId);
            }
        }

        public void Store(Entry entry)
        {
            lock (_cache)
            {
                _cache[entry.Key] = entry;
            }
        }

        public class Key
        {
            public SparkViewDescriptor Descriptor { get; set; }

            public override int GetHashCode()
            {
                int hashCode = 0;

                hashCode ^= (Descriptor.TargetNamespace ?? "").GetHashCode();

                foreach (var template in Descriptor.Templates)
                    hashCode ^= template.ToLowerInvariant().GetHashCode();

                return hashCode;
            }

            public override bool Equals(object obj)
            {
                var that = obj as Key;
                if (that == null || GetType() != that.GetType())
                    return false;
                if (!string.Equals(Descriptor.TargetNamespace, that.Descriptor.TargetNamespace))
                    return false;
                if (Descriptor.Templates.Count != that.Descriptor.Templates.Count)
                    return false;
                for (int index = 0; index != Descriptor.Templates.Count; ++index)
                {
                    if (!string.Equals(Descriptor.Templates[index], that.Descriptor.Templates[index], StringComparison.InvariantCultureIgnoreCase))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public class Entry : ISparkViewEntry
        {
            public Key Key { get; set; }
            public ViewLoader Loader { get; set; }
            public ViewCompiler Compiler { get; set; }
            public IViewActivator Activator { get; set; }
            public ISparkLanguageFactory LanguageFactory { get; set; }

            public SparkViewDescriptor Descriptor
            {
                get { return Key.Descriptor; }
            }

            public string SourceCode
            {
                get { return Compiler.SourceCode; }
            }

            public IList<SourceMapping> SourceMappings
            {
                get { return Compiler.SourceMappings; }
            }

            public ISparkView CreateInstance()
            {
                var view = Activator.Activate(Compiler.CompiledType);
                if (LanguageFactory != null)
                    LanguageFactory.InstanceCreated(Compiler, view);
                return view;
            }

            public void ReleaseInstance(ISparkView view)
            {
                if (LanguageFactory != null)
                    LanguageFactory.InstanceReleased(Compiler, view);
                Activator.Release(Compiler.CompiledType, view);
            }
        }
    }
}

