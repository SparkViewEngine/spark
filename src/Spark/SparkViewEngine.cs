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
using System.Configuration;
using System.Reflection;
using Spark.Bindings;
using Spark.Compiler;
using Spark.Compiler.CSharp;
using Spark.Parser;
using Spark.FileSystem;
using Spark.Parser.Syntax;

namespace Spark
{
    public class SparkViewEngine : ISparkViewEngine, ISparkServiceInitialize
    {
        public SparkViewEngine()
            : this(null)
        {
        }

        public SparkViewEngine(ISparkSettings settings)
        {
            Settings = settings ??
                (ISparkSettings)ConfigurationManager.GetSection("spark") ??
                throw new ConfigurationErrorsException(SparkServiceContainer.MissingSparkSettingsConfigurationErrorExceptionMessage);
            SyntaxProvider = new DefaultSyntaxProvider(Settings);
            ViewActivatorFactory = new DefaultViewActivator();
        }

        public void Initialize(ISparkServiceContainer container)
        {
            Settings = container.GetService<ISparkSettings>();
            SyntaxProvider = container.GetService<ISparkSyntaxProvider>();
            ViewActivatorFactory = container.GetService<IViewActivatorFactory>();
            LanguageFactory = container.GetService<ISparkLanguageFactory>();
            BindingProvider = container.GetService<IBindingProvider>();
            ResourcePathManager = container.GetService<IResourcePathManager>();
            TemplateLocator = container.GetService<ITemplateLocator>();
            CompiledViewHolder = container.GetService<ICompiledViewHolder>();
            PartialProvider = container.GetService<IPartialProvider>();
            PartialReferenceProvider = container.GetService<IPartialReferenceProvider>();
            SetViewFolder(container.GetService<IViewFolder>());
        }

        private IViewFolder _viewFolder;
        public IViewFolder ViewFolder
        {
            get
            {
                if (_viewFolder == null)
                {
                    SetViewFolder(Settings.CreateDefaultViewFolder());
                }

                return _viewFolder;
            }
            set { SetViewFolder(value); }
        }

        private ISparkLanguageFactory _langaugeFactory;
        public ISparkLanguageFactory LanguageFactory
        {
            get
            {
                if (_langaugeFactory == null)
                    _langaugeFactory = new DefaultLanguageFactory();
                return _langaugeFactory;
            }
            set { _langaugeFactory = value; }
        }


        private IBindingProvider _bindingProvider;
        public IBindingProvider BindingProvider
        {
            get
            {
                if (_bindingProvider == null)
                    _bindingProvider = new DefaultBindingProvider();
                return _bindingProvider;
            }
            set { _bindingProvider = value; }
        }

        private IPartialProvider _partialProvider;
        public IPartialProvider PartialProvider
        {
            get
            {
                if (_partialProvider == null)
                    _partialProvider = new DefaultPartialProvider();
                return _partialProvider;
            }
            set { _partialProvider = value; }
        }

        private IPartialReferenceProvider _partialReferenceProvider;
        public IPartialReferenceProvider PartialReferenceProvider
        {
            get
            {
                if (_partialReferenceProvider == null)
                    _partialReferenceProvider = new DefaultPartialReferenceProvider(() => PartialProvider);
                return _partialReferenceProvider;
            }
            set { _partialReferenceProvider = value; }
        }
        
        private void SetViewFolder(IViewFolder value)
        {
            var aggregateViewFolder = value;
            
            if (this.Settings.ViewFolders != null)
            {
                foreach (var viewFolderSettings in this.Settings.ViewFolders)
                {
                    IViewFolder viewFolder = this.ActivateViewFolder(viewFolderSettings);
                    
                    if (!string.IsNullOrEmpty(viewFolderSettings.Subfolder))
                    {
                        viewFolder = new SubViewFolder(viewFolder, viewFolderSettings.Subfolder);
                    }

                    aggregateViewFolder = aggregateViewFolder.Append(viewFolder);
                }
            }
            
            _viewFolder = aggregateViewFolder;
        }

        private IViewFolder ActivateViewFolder(IViewFolderSettings viewFolderSettings)
        {
            var type = Type.GetType(viewFolderSettings.Type);
            
            ConstructorInfo bestConstructor = null;
            foreach (var constructor in type.GetConstructors())
            {
                if (bestConstructor == null || bestConstructor.GetParameters().Length < constructor.GetParameters().Length)
                {
                    if (constructor.GetParameters().All(param => viewFolderSettings.Parameters.ContainsKey(param.Name)))
                    {
                        bestConstructor = constructor;
                    }
                }
            }

            if (bestConstructor == null)
            {
                throw new MissingMethodException($"No suitable constructor for {type.FullName} located");
            }

            var args = bestConstructor.GetParameters()
                .Select(param => ChangeType(viewFolderSettings, param))
                .ToArray();

            return (IViewFolder)Activator.CreateInstance(type, args);
        }

        private object ChangeType(IViewFolderSettings viewFolderSettings, ParameterInfo param)
        {
            if (param.ParameterType == typeof(Assembly))
                return Assembly.Load(viewFolderSettings.Parameters[param.Name]);

            return Convert.ChangeType(viewFolderSettings.Parameters[param.Name], param.ParameterType);
        }

        private IResourcePathManager _resourcePathManager;
        public IResourcePathManager ResourcePathManager
        {
            get
            {
                if (_resourcePathManager == null)
                    _resourcePathManager = new DefaultResourcePathManager(Settings);
                return _resourcePathManager;
            }
            set { _resourcePathManager = value; }
        }

        public ISparkExtensionFactory ExtensionFactory { get; set; }
        public IViewActivatorFactory ViewActivatorFactory { get; set; }

        private ITemplateLocator _templateLocator;
        public ITemplateLocator TemplateLocator
        {
            get
            {
                if (_templateLocator == null)
                    _templateLocator = new DefaultTemplateLocator();
                return _templateLocator;
            }
            set { _templateLocator = value; }
        }

        private ICompiledViewHolder _compiledViewHolder;
        public ICompiledViewHolder CompiledViewHolder
        {
            get
            {
                if (_compiledViewHolder == null)
                    _compiledViewHolder = new CompiledViewHolder();
                return _compiledViewHolder;
            }
            set { _compiledViewHolder = value; }
        }

        public ISparkSyntaxProvider SyntaxProvider { get; set; }

        public ISparkSettings Settings { get; set; }
        public string DefaultPageBaseType { get; set; }

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
            if (view == null) throw new ArgumentNullException("view");

            var entry = CompiledViewHolder.Lookup(view.GeneratedViewId);
            if (entry != null)
                entry.ReleaseInstance(view);
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

        private ViewLoader CreateViewLoader()
        {
            return new ViewLoader
            {
                ViewFolder = ViewFolder,
                SyntaxProvider = SyntaxProvider,
                ExtensionFactory = ExtensionFactory,
                Prefix = Settings.Prefix,
                BindingProvider = BindingProvider,
                ParseSectionTagAsSegment = Settings.ParseSectionTagAsSegment,
                AttributeBehaviour = Settings.AttributeBehaviour,
                PartialProvider = PartialProvider,
                PartialReferenceProvider = PartialReferenceProvider
            };
        }

        public Assembly BatchCompilation(IList<SparkViewDescriptor> descriptors)
        {
            return BatchCompilation(null /*outputAssembly*/, descriptors);
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
                    Loader = CreateViewLoader(),
                    Compiler = LanguageFactory.CreateViewCompiler(this, descriptor)
                };

                var chunksLoaded = new List<IList<Chunk>>();
                var templatesLoaded = new List<string>();
                LoadTemplates(entry.Loader, descriptor.Templates, chunksLoaded, templatesLoaded);

                entry.Compiler.GenerateSourceCode(chunksLoaded, entry.Loader.GetEverythingLoaded());
                sourceCode.Add(entry.Compiler.SourceCode);

                batch.Add(entry);
            }

            var batchCompiler = new BatchCompiler { OutputAssembly = outputAssembly };

            var assembly = batchCompiler.Compile(Settings.Debug, "csharp", sourceCode.ToArray());
            foreach (var entry in batch)
            {
                entry.Compiler.CompiledType = assembly.GetType(entry.Compiler.ViewClassFullName);
                entry.Activator = ViewActivatorFactory.Register(entry.Compiler.CompiledType);

                CompiledViewHolder.Store(entry);
            }

            return assembly;
        }

        public IList<SparkViewDescriptor> LoadBatchCompilation(Assembly assembly)
        {
            var descriptors = new List<SparkViewDescriptor>();

            foreach (var type in assembly.GetExportedTypes())
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
                                    Loader = new ViewLoader { PartialProvider = PartialProvider, PartialReferenceProvider = PartialReferenceProvider },
                                    Compiler = new CSharpViewCompiler { CompiledType = type },
                                    Activator = ViewActivatorFactory.Register(type)
                                };
                CompiledViewHolder.Store(entry);

                descriptors.Add(descriptor);
            }

            return descriptors;
        }
    }
}
