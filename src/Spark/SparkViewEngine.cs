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
using System.Collections.Generic;
using System.Reflection;
using Spark.Bindings;
using Spark.Compiler;
using Spark.Compiler.CSharp;
using Spark.Parser;
using Spark.FileSystem;

namespace Spark
{
    public class SparkViewEngine : ISparkViewEngine
    {
        public ISparkSettings Settings { get; }
        public IViewFolder ViewFolder { get; set; }
        public readonly ISparkLanguageFactory LanguageFactory;
        public IViewActivatorFactory ViewActivatorFactory { get; }
        public readonly ICompiledViewHolder CompiledViewHolder;

        public ISparkSyntaxProvider SyntaxProvider { get; }

        private readonly IBatchCompiler BatchCompiler;
        private readonly IPartialProvider PartialProvider;
        private readonly IPartialReferenceProvider PartialReferenceProvider;
        private readonly IBindingProvider BindingProvider;
        private readonly ISparkExtensionFactory SparkExtensionFactory;

        public SparkViewEngine(
            ISparkSettings settings, 
            ISparkSyntaxProvider syntaxProvider,
            IViewActivatorFactory viewActivatorFactory,
            ISparkLanguageFactory languageFactory,
            ICompiledViewHolder compiledViewHolder,
            IViewFolder viewFolder,
            IBatchCompiler batchCompiler,
            IPartialProvider partialProvider,
            IPartialReferenceProvider partialReferenceProvider,
            IBindingProvider bindingProvider,
            ISparkExtensionFactory sparkExtensionFactory)
        {
            Settings = settings;

            SyntaxProvider = syntaxProvider;
            ViewActivatorFactory = viewActivatorFactory;
            LanguageFactory = languageFactory;
            
            CompiledViewHolder = compiledViewHolder;
            
            ViewFolder = this.InitialiseAggregateViewFolder(settings, viewFolder);

            BatchCompiler = batchCompiler;
            PartialProvider = partialProvider;
            PartialReferenceProvider = partialReferenceProvider;
            BindingProvider = bindingProvider;
            SparkExtensionFactory = sparkExtensionFactory;
        }

        private IViewFolder InitialiseAggregateViewFolder(ISparkSettings settings, IViewFolder value)
        {
            var aggregateViewFolder = value;
            
            if (settings.ViewFolders != null)
            {
                foreach (var viewFolderSettings in settings.ViewFolders)
                {
                    IViewFolder viewFolder = viewFolderSettings.ActivateViewFolder();
                    
                    if (!string.IsNullOrEmpty(viewFolderSettings.Subfolder))
                    {
                        viewFolder = new SubViewFolder(viewFolder, viewFolderSettings.Subfolder);
                    }

                    aggregateViewFolder = aggregateViewFolder.Append(viewFolder);
                }
            }
            
            return aggregateViewFolder;
        }

        public ISparkViewEntry GetEntry(SparkViewDescriptor descriptor)
        {
            return CompiledViewHolder.Lookup(descriptor);
        }

        public ISparkView CreateInstance(SparkViewDescriptor descriptor)
        {
            return CreateEntry(descriptor).CreateInstance();
        }

        public void ReleaseInstance(ISparkView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            var entry = CompiledViewHolder.Lookup(view.GeneratedViewId);

            entry?.ReleaseInstance(view);
        }

        public ISparkViewEntry CreateEntry(SparkViewDescriptor descriptor)
        {
            var entry = CompiledViewHolder.Lookup(descriptor);

            if (entry == null)
            {
                entry = CreateEntryInternal(descriptor, true);

                CompiledViewHolder.Store(entry);
            }

            return entry;
        }

        public ISparkViewEntry CreateEntryInternal(SparkViewDescriptor descriptor, bool compile)
        {
            var entry = new CompiledViewEntry
            {
                Descriptor = descriptor,
                Loader = CreateViewLoader(),
                Compiler = LanguageFactory.CreateViewCompiler(this, descriptor),
                LanguageFactory = LanguageFactory
            };

            var chunksLoaded = new List<IList<Chunk>>();
            var templatesLoaded = new List<string>();

            LoadTemplates(entry.Loader, entry.Descriptor.Templates, chunksLoaded, templatesLoaded);

            if (compile)
            {
                entry.Compiler.CompileView(chunksLoaded, entry.Loader.GetEverythingLoaded());

                entry.Activator = ViewActivatorFactory.Register(entry.Compiler.CompiledType);
            }
            else
            {
                entry.Compiler.GenerateSourceCode(chunksLoaded, entry.Loader.GetEverythingLoaded());
            }

            return entry;
        }

        void LoadTemplates(ViewLoader loader, IEnumerable<string> templates, ICollection<IList<Chunk>> chunksLoaded, ICollection<string> templatesLoaded)
        {
            foreach (var template in templates)
            {
                if (templatesLoaded.Contains(template))
                {
                    throw new CompilerException($"Unable to include template '{templates}' recursively");
                }

                var chunks = loader.Load(template);
                chunksLoaded.Add(chunks);
                templatesLoaded.Add(template);
            }
        }

        public Assembly BatchCompilation(IList<SparkViewDescriptor> descriptors)
        {
            return BatchCompilation(null /*outputAssembly*/, descriptors);
        }

        /// <summary>
        /// ViewLoader must be transient (due to its dictionary and list).
        /// </summary>
        /// <returns></returns>
        private ViewLoader CreateViewLoader()
        {
            return new ViewLoader(
                this.Settings,
                this.ViewFolder,
                this.PartialProvider,
                this.PartialReferenceProvider,
                this.SparkExtensionFactory,
                this.SyntaxProvider,
                this.BindingProvider);
        }

        public Assembly BatchCompilation(string outputAssembly, IList<SparkViewDescriptor> descriptors)
        {
            var batch = new List<CompiledViewEntry>();
            var sourceCode = new List<string>();

            foreach (var descriptor in descriptors)
            {
                var entry = new CompiledViewEntry
                {
                    Descriptor = descriptor,
                    Loader = this.CreateViewLoader(),
                    Compiler = LanguageFactory.CreateViewCompiler(this, descriptor)
                };

                var chunksLoaded = new List<IList<Chunk>>();
                var templatesLoaded = new List<string>();
                LoadTemplates(entry.Loader, descriptor.Templates, chunksLoaded, templatesLoaded);

                entry.Compiler.GenerateSourceCode(chunksLoaded, entry.Loader.GetEverythingLoaded());
                sourceCode.Add(entry.Compiler.SourceCode);

                batch.Add(entry);
            }

            var assembly = BatchCompiler.Compile(Settings.Debug, "csharp", outputAssembly, sourceCode, Settings.ExcludeAssemblies);

            foreach (var entry in batch)
            {
                entry.Compiler.CompiledType = assembly.GetType(entry.Compiler.ViewClassFullName);
                entry.Activator = ViewActivatorFactory.Register(entry.Compiler.CompiledType);

                CompiledViewHolder.Store(entry);
            }

            return assembly;
        }

        /// <summary>
        /// Get all the exported types in the assembly and loads the ones assignable from <see cref="ISparkView"/>.
        /// </summary>
        /// <param name="assembly"></param>
        /// <seealso cref="LoadBatchCompilation(Type[])"/>
        /// <returns>A list of <see cref="SparkViewDescriptor"/> for every loaded type.</returns>
        public IList<SparkViewDescriptor> LoadBatchCompilation(Assembly assembly)
        {
            var exportedTypes = assembly.GetExportedTypes();

            return LoadBatchCompilation(exportedTypes);
        }

        /// <summary>
        /// Loads the specified types (when assignable from <see cref="ISparkView"/>) into the <see cref="ICompiledViewHolder"/> implemenation.
        /// </summary>
        /// <param name="types"></param>
        /// <returns>A list of <see cref="SparkViewDescriptor"/> for every loaded type.</returns>
        public IList<SparkViewDescriptor> LoadBatchCompilation(Type[] types)
        {
            var descriptors = new List<SparkViewDescriptor>();

            foreach (var type in types)
            {
                if (!typeof(ISparkView).IsAssignableFrom(type))
                {
                    continue;
                }

                var attributes = type.GetCustomAttributes(typeof(SparkViewAttribute), false);
                if (attributes == null || attributes.Length == 0)
                {
                    continue;
                }

                var descriptor = ((SparkViewAttribute)attributes[0]).BuildDescriptor();

                var entry = new CompiledViewEntry
                {
                    Descriptor = descriptor,
                    Loader = this.CreateViewLoader(),
                    Compiler = new CSharpViewCompiler(this.BatchCompiler, this.Settings) { CompiledType = type },
                    Activator = ViewActivatorFactory.Register(type)
                };

                CompiledViewHolder.Store(entry);

                descriptors.Add(descriptor);
            }

            return descriptors;
        }
    }
}
