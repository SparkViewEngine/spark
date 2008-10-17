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
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web.Hosting;
using Spark.Compiler;
using Spark.Compiler.CSharp;
using Spark.Compiler.Javascript;
using Spark.Parser;
using Spark;
using Spark.FileSystem;
using Spark.Parser.Syntax;

namespace Spark
{
    public class SparkViewEngine : ISparkViewEngine
    {
        public SparkViewEngine()
            : this(null)
        {
        }

        public SparkViewEngine(ISparkSettings settings)
        {
            Settings = settings ?? (ISparkSettings)ConfigurationManager.GetSection("spark") ?? new SparkSettings();
            SyntaxProvider = new DefaultSyntaxProvider();
            ViewActivatorFactory = new DefaultViewActivator();
        }

        private IViewFolder _viewFolder;
        public IViewFolder ViewFolder
        {
            get
            {
                if (_viewFolder == null)
                    SetViewFolder(DefaultViewFolder());
                return _viewFolder;
            }
            set { SetViewFolder(value); }
        }

        private static IViewFolder DefaultViewFolder()
        {
            if (HostingEnvironment.IsHosted && HostingEnvironment.VirtualPathProvider != null)
                return new VirtualPathProviderViewFolder("~/Views");
            var appBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            return new FileSystemViewFolder(Path.Combine(appBase, "Views"));
        }

        private void SetViewFolder(IViewFolder value)
        {
            var aggregateViewFolder = value;
            foreach(var viewFolderSettings in Settings.ViewFolders)
            {
                IViewFolder viewFolder = ActivateViewFolder(viewFolderSettings);
                if (!string.IsNullOrEmpty(viewFolderSettings.Subfolder))
                    viewFolder = new SubViewFolder(viewFolder, viewFolderSettings.Subfolder);
                aggregateViewFolder = aggregateViewFolder.Append(viewFolder);

            }
            _viewFolder = aggregateViewFolder;
        }

        private IViewFolder ActivateViewFolder(IViewFolderSettings viewFolderSettings)
        {
            Type type;
            switch(viewFolderSettings.FolderType)
            {
                case ViewFolderType.FileSystem:
                    type = typeof (FileSystemViewFile);
                    break;
                case ViewFolderType.EmbeddedResource:
                    type = typeof(EmbeddedViewFolder);
                    break;
                case ViewFolderType.VirtualPathProvider:
                    type = typeof (VirtualPathProviderViewFolder);
                    break;
                case ViewFolderType.Custom:
                    type = Type.GetType(viewFolderSettings.Type);
                    break;
                default:
                    throw new ArgumentException("Unknown value for view folder type");
            }

            ConstructorInfo bestConstructor = null;
            foreach(var constructor in type.GetConstructors())
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
                throw new MissingMethodException(string.Format("No suitable constructor for {0} located", type.FullName));
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

        public ISparkSyntaxProvider SyntaxProvider { get; set; }

        public ISparkSettings Settings { get; set; }
        public string DefaultPageBaseType { get; set; }

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

        public void ReleaseInstance(ISparkView view)
        {
            if (view == null) throw new ArgumentNullException("view");

            var entry = CompiledViewHolder.Current.Lookup(view.GeneratedViewId);
            if (entry != null)
                entry.ReleaseInstance(view);
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
                                Loader = CreateViewLoader(),
                                Compiler = CreateViewCompiler(key.Descriptor)
                            };


            var chunks = new List<IList<Chunk>>();

            foreach (var template in key.Descriptor.Templates)
                chunks.Add(entry.Loader.Load(template));

            entry.Compiler.CompileView(chunks, entry.Loader.GetEverythingLoaded());

            entry.Activator = ViewActivatorFactory.Register(entry.Compiler.CompiledType);

            return entry;
        }

        private ViewLoader CreateViewLoader()
        {
            return new ViewLoader
                       {
                           ViewFolder = ViewFolder,
                           SyntaxProvider = SyntaxProvider,
                           ExtensionFactory = ExtensionFactory,
                           Prefix = Settings.Prefix
                       };
        }

        private ViewCompiler CreateViewCompiler(SparkViewDescriptor descriptor)
        {
            var pageBaseType = Settings.PageBaseType;
            if (string.IsNullOrEmpty(pageBaseType))
                pageBaseType = DefaultPageBaseType;

            ViewCompiler viewCompiler;
            switch (descriptor.Language)
            {
                case LanguageType.CSharp:
                    viewCompiler = new DefaultViewCompiler();
                    break;
                case LanguageType.Javascript:
                    viewCompiler = new JavascriptViewCompiler();
                    break;
                default:
                    throw new CompilerException(string.Format("Unknown language type {0}", descriptor.Language));
            }

            viewCompiler.BaseClass = pageBaseType;
            viewCompiler.Descriptor = descriptor;
            viewCompiler.Debug = Settings.Debug;
            viewCompiler.UseAssemblies = Settings.UseAssemblies;
            viewCompiler.UseNamespaces = Settings.UseNamespaces;
            return viewCompiler;
        }

        public Assembly BatchCompilation(IList<SparkViewDescriptor> descriptors)
        {
            return BatchCompilation(null /*outputAssembly*/, descriptors);
        }

        public Assembly BatchCompilation(string outputAssembly, IList<SparkViewDescriptor> descriptors)
        {
            var batch = new List<CompiledViewHolder.Entry>();
            var sourceCode = new List<string>();

            foreach (var descriptor in descriptors)
            {
                var entry = new CompiledViewHolder.Entry
                {
                    Key = CreateKey(descriptor),
                    Loader = CreateViewLoader(),
                    Compiler = CreateViewCompiler(descriptor)
                };

                var templateChunks = new List<IList<Chunk>>();
                foreach (var template in descriptor.Templates)
                    templateChunks.Add(entry.Loader.Load(template));

                entry.Compiler.GenerateSourceCode(templateChunks, entry.Loader.GetEverythingLoaded());
                sourceCode.Add(entry.Compiler.SourceCode);

                batch.Add(entry);
            }

            var batchCompiler = new BatchCompiler{OutputAssembly = outputAssembly};

            var assembly = batchCompiler.Compile(Settings.Debug, sourceCode.ToArray());
            foreach (var entry in batch)
            {
                entry.Compiler.CompiledType = assembly.GetType(entry.Compiler.ViewClassFullName);
                entry.Activator = ViewActivatorFactory.Register(entry.Compiler.CompiledType);
                CompiledViewHolder.Current.Store(entry);
            }
            return assembly;
        }

        public IList<SparkViewDescriptor> LoadBatchCompilation(Assembly assembly)
        {
            var descriptors = new List<SparkViewDescriptor>();

            foreach (var type in assembly.GetExportedTypes())
            {
                if (!typeof(ISparkView).IsAssignableFrom(type))
                    continue;

                var attributes = type.GetCustomAttributes(typeof(SparkViewAttribute), false);
                if (attributes == null || attributes.Length == 0)
                    continue;

                var descriptor = ((SparkViewAttribute)attributes[0]).BuildDescriptor();

                var entry = new CompiledViewHolder.Entry
                                {
                                    Key = new CompiledViewHolder.Key { Descriptor = descriptor },
                                    Loader = new ViewLoader(),
                                    Compiler = new DefaultViewCompiler { CompiledType = type },
                                    Activator = ViewActivatorFactory.Register(type)
                                };
                CompiledViewHolder.Current.Store(entry);

                descriptors.Add(descriptor);
            }

            return descriptors;
        }
    }
}
