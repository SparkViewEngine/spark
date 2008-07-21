/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Collections.Generic;
using Spark.Compiler;
using Spark.Parser;
using Spark;
using Spark.FileSystem;

namespace Spark
{
    public class SparkViewEngine : ISparkViewEngine
    {
        public SparkViewEngine(string baseClass, IViewFolder viewFolder)
        {
            BaseClass = baseClass;
            ViewFolder = viewFolder;
        }

        public string BaseClass { get; set; }
        public IViewFolder ViewFolder { get; set; }
        public ISparkExtensionFactory ExtensionFactory { get; set; }


        public ISparkViewEntry GetEntry(SparkViewDescriptor descriptor)
        {
            var key = CreateKey(descriptor);
            return CompiledViewHolder.Current.Lookup(key);
        }

        public ISparkViewEntry CreateEntry(SparkViewDescriptor descriptor)
        {
            var key = CreateKey(descriptor);
            var entry = CompiledViewHolder.Current.Lookup(key);
            if (entry == null)
            {
                entry = CreateEntry(key);
                CompiledViewHolder.Current.Store(entry);
            }
            return entry;
        }

        public ISparkView CreateInstance(SparkViewDescriptor descriptor)
        {
            return CreateEntry(descriptor).CreateInstance();
        }


        public CompiledViewHolder.Key CreateKey(SparkViewDescriptor descriptor)
        {
            //TODO: get this logic out of here. it would be much better for the framework-specific 
            // library to do the work of locating these defaults

            var key = new CompiledViewHolder.Key
                        {
                            Descriptor = descriptor
                        };

            if (descriptor.ViewName == null)
                descriptor.ViewName = string.Empty;
            if (descriptor.ControllerName == null)
                descriptor.ControllerName = string.Empty;
            if (descriptor.MasterName == null)
                descriptor.MasterName = string.Empty;

            if (key.Descriptor.MasterName == string.Empty)
            {
                if (ViewFolder.HasView(string.Format("Shared\\{0}.spark", key.Descriptor.ControllerName)))
                {
                    key.Descriptor.MasterName = key.Descriptor.ControllerName;
                }
                else if (ViewFolder.HasView("Shared\\Application.spark"))
                {
                    key.Descriptor.MasterName = "Application";
                }
            }
            return key;
        }

        public CompiledViewHolder.Entry CreateEntry(CompiledViewHolder.Key key)
        {
            var entry = new CompiledViewHolder.Entry
                            {
                                Key = key,
                                Loader = new ViewLoader { ViewFolder = ViewFolder, ExtensionFactory = ExtensionFactory },
                                Compiler = new ViewCompiler(BaseClass)
                            };

            var viewChunks = entry.Loader.Load(key.Descriptor.ControllerName, key.Descriptor.ViewName);

            IList<Chunk> masterChunks = new Chunk[0];
            if (!string.IsNullOrEmpty(key.Descriptor.MasterName))
                masterChunks = entry.Loader.Load("Shared", key.Descriptor.MasterName);

            entry.Compiler.CompileView(viewChunks, masterChunks);

            return entry;
        }

    }
}