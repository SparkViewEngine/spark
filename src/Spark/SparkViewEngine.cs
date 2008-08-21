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

using System;
using System.Collections.Generic;
using System.Configuration;
using Spark.Compiler;
using Spark.Parser;
using Spark;
using Spark.FileSystem;
using Spark.Parser.Syntax;

namespace Spark
{
    public class SparkViewEngine : ISparkViewEngine
    {
        public SparkViewEngine() : this(null)
        {
        }

        public SparkViewEngine(ISparkSettings settings)
        {
            Settings = settings ?? (ISparkSettings)ConfigurationManager.GetSection("spark") ?? new SparkSettings();
            SyntaxProvider = new DefaultSyntaxProvider();
            ViewActivatorFactory = new DefaultViewActivator();
        }

        public IViewFolder ViewFolder { get; set; }
        public ISparkExtensionFactory ExtensionFactory { get; set; }
        public IViewActivatorFactory ViewActivatorFactory { get; set; }

        public ISparkSyntaxProvider SyntaxProvider { get; set; }

        public ISparkSettings Settings { get; set; }

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
            return new CompiledViewHolder.Key
                        {
                            Descriptor = descriptor
                        };
        }

        public CompiledViewHolder.Entry CreateEntry(CompiledViewHolder.Key key)
        {
            var entry = new CompiledViewHolder.Entry
                            {
                                Key = key,
                                Loader = new ViewLoader
                                {
                                    ViewFolder = ViewFolder,
                                    SyntaxProvider = SyntaxProvider,
                                    ExtensionFactory = ExtensionFactory
                                },
                                Compiler = new ViewCompiler(Settings.PageBaseType, key.Descriptor.TargetNamespace)
                                {
                                    Debug = Settings.Debug,
                                    UseAssemblies = Settings.UseAssemblies,
                                    UseNamespaces = Settings.UseNamespaces
                                }
                            };


            var chunks = new List<IList<Chunk>>();

            foreach (var template in key.Descriptor.Templates)
                chunks.Add(entry.Loader.Load(template));

            entry.Compiler.CompileView(chunks, entry.Loader.GetEverythingLoaded());

            entry.Activator = ViewActivatorFactory.Register(entry.Compiler.CompiledType);

            return entry;
        }

    }
}
