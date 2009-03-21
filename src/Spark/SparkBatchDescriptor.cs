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
using System.Collections.Generic;
using System.Reflection;

namespace Spark
{
    public class SparkBatchDescriptor
    {
        public SparkBatchDescriptor()
            : this(null /*assemblyName*/)
        {
        }

        public SparkBatchDescriptor(string assemblyName)
        {
            OutputAssembly = assemblyName;
            Entries = new List<SparkBatchEntry>();
        }

        public string OutputAssembly { get; set; }
        public IList<SparkBatchEntry> Entries { get; set; }

        public SparkBatchConfigurator For(Type controllerType)
        {
            var entry = new SparkBatchEntry { ControllerType = controllerType };
            Entries.Add(entry);
            return new SparkBatchConfigurator(this, entry);
        }

        public SparkBatchConfigurator For<TController>()
        {
            return For(typeof(TController));
        }

        public SparkBatchDescriptor FromAttributes<TController>()
        {
            return FromAttributes(typeof(TController));
        }

        public SparkBatchDescriptor FromAttributes(Type controllerType)
        {
            var precompileAttributes = controllerType.GetCustomAttributes(typeof(PrecompileAttribute), true);
            foreach (PrecompileAttribute precompileAttribute in precompileAttributes ?? new object[0])
            {
                var config = For(controllerType);
                foreach (var item in SplitParts(precompileAttribute.Include))
                {
                    config.Include(item);
                }
                foreach (var item in SplitParts(precompileAttribute.Exclude))
                {
                    config.Exclude(item);
                }
                foreach (var item in SplitParts(precompileAttribute.Layout))
                {
                    config.Layout(item.Split('+'));
                }
            }
            return this;
        }

        private static string[] SplitParts(string value)
        {
            return (value ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public SparkBatchDescriptor FromAssemblyNamed(string assemblyString)
        {
            return FromAssembly(Assembly.Load(assemblyString));
        }

        public SparkBatchDescriptor FromAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetExportedTypes())
                FromAttributes(type);

            return this;
        }
    }

    public class SparkBatchEntry
    {
        public SparkBatchEntry()
        {
            LayoutNames = new List<IList<string>>();
            IncludeViews = new List<string>();
            ExcludeViews = new List<string>();
        }

        public Type ControllerType { get; set; }
        public IList<IList<string>> LayoutNames { get; set; }
        public IList<string> IncludeViews { get; set; }
        public IList<string> ExcludeViews { get; set; }
    }

    public sealed class SparkBatchConfigurator
    {
        private readonly SparkBatchDescriptor descriptor;
        private readonly SparkBatchEntry entry;

        internal SparkBatchConfigurator(SparkBatchDescriptor descriptor, SparkBatchEntry entry)
        {
            this.descriptor = descriptor;
            this.entry = entry;
        }

        public SparkBatchDescriptor FromAssemblyNamed(string assemblyString)
        {
            return descriptor.FromAssemblyNamed(assemblyString);
        }
        public SparkBatchDescriptor FromAssembly(Assembly assembly)
        {
            return descriptor.FromAssembly(assembly);
        }

        public SparkBatchDescriptor FromAttributes<TController>()
        {
            return descriptor.FromAttributes<TController>();
        }

        public SparkBatchDescriptor FromAttributes(Type controllerType)
        {
            return descriptor.FromAttributes(controllerType);
        }

        public SparkBatchConfigurator For(Type controllerType, params string[] layoutNames)
        {
            return descriptor.For(controllerType);
        }

        public SparkBatchConfigurator For<TController>(params string[] layoutNames)
        {
            return descriptor.For<TController>();
        }

        public SparkBatchConfigurator Layout(params string[] layouts)
        {
            entry.LayoutNames.Add(layouts);
            return this;
        }

        public SparkBatchConfigurator Include(string pattern)
        {
            entry.IncludeViews.Add(pattern);
            return this;
        }

        public SparkBatchConfigurator Exclude(string pattern)
        {
            entry.ExcludeViews.Add(pattern);
            return this;
        }
    }
}
