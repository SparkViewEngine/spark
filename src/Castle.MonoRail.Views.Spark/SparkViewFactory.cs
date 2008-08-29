// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
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

using System.Reflection;
using Castle.Core.Logging;
using Spark.Compiler;

namespace Castle.MonoRail.Views.Spark
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Castle.MonoRail.Framework;

    using global::Spark;
    using global::Spark.FileSystem;
    using global::Spark.Parser.Markup;

    public class SparkViewFactory : ViewEngineBase, IViewFolder, ISparkExtensionFactory
    {
        private IControllerDescriptorProvider _controllerDescriptorProvider;
        private IViewActivatorFactory _viewActivatorFactory;

        public override void Service(IServiceProvider provider)
        {
            base.Service(provider);

            _controllerDescriptorProvider = (IControllerDescriptorProvider)provider.GetService(typeof(IControllerDescriptorProvider));
            _viewActivatorFactory = (IViewActivatorFactory)provider.GetService(typeof (IViewActivatorFactory));

            Engine = (ISparkViewEngine)provider.GetService(typeof(ISparkViewEngine));
        }

        private ISparkViewEngine _engine;
        public ISparkViewEngine Engine
        {
            get
            {
                if (_engine == null)
                    Engine = new SparkViewEngine();
                return _engine;
            }
            set
            {
                _engine = value;
                if (_engine != null)
                {
                    _engine.ViewFolder = this;
                    _engine.ExtensionFactory = this;
                    _engine.DefaultPageBaseType = typeof(SparkView).FullName;
                    
                    if (_viewActivatorFactory != null)
                        _engine.ViewActivatorFactory = _viewActivatorFactory;
                }
            }
        }

        public override string ViewFileExtension
        {
            get { return "spark"; }
        }

        public override bool HasTemplate(string templateName)
        {
            return base.HasTemplate(Path.ChangeExtension(templateName, ViewFileExtension));
        }

        public override void Process(string templateName, TextWriter output, IEngineContext context, IController controller,
                                     IControllerContext controllerContext)
        {
            //string masterName = null;
            //if (controllerContext.LayoutNames != null)
            //    masterName = string.Join(" ", controllerContext.LayoutNames);

            var descriptor = new SparkViewDescriptor { TargetNamespace = controller.GetType().Namespace };
            descriptor.Templates.Add(Path.ChangeExtension(templateName, ViewFileExtension));

            foreach (var layoutName in controllerContext.LayoutNames ?? new string[0])
            {
                if (HasTemplate("Layouts\\" + layoutName))
                {
                    descriptor.Templates.Add(Path.ChangeExtension("Layouts\\" + layoutName, ViewFileExtension));
                }
                else if (HasTemplate("Shared\\" + layoutName))
                {
                    descriptor.Templates.Add(Path.ChangeExtension("Shared\\" + layoutName, ViewFileExtension));
                }
                else
                {
                    throw new CompilerException(string.Format(
                                                    "Unable to find templates layouts\\{0} or shared\\{0}",
                                                    layoutName));
                }
            }

            var entry = Engine.CreateEntry(descriptor);
            var view = (SparkView)entry.CreateInstance();
            view.Contextualize(context, controllerContext, this);
            if (view.Logger == null || view.Logger == NullLogger.Instance)
                view.Logger = Logger;
            view.RenderView(output);
            entry.ReleaseInstance(view);
        }

        public override void Process(string templateName, string layoutName, TextWriter output,
                                     IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public override void ProcessPartial(string partialName, TextWriter output, IEngineContext context,
                                            IController controller, IControllerContext controllerContext)
        {
            throw new NotImplementedException();
        }

        public override void RenderStaticWithinLayout(string contents, IEngineContext context, IController controller,
                                                      IControllerContext controllerContext)
        {
            throw new NotImplementedException();
        }

        public override bool SupportsJSGeneration
        {
            get { return false; }
        }

        public override string JSGeneratorFileExtension
        {
            get { return null; }
        }

        public override object CreateJSGenerator(JSCodeGeneratorInfo generatorInfo, IEngineContext context,
                                                 IController controller, IControllerContext controllerContext)
        {
            throw new NotImplementedException();
        }

        public override void GenerateJS(string templateName, TextWriter output, JSCodeGeneratorInfo generatorInfo,
                                        IEngineContext context, IController controller, IControllerContext controllerContext)
        {
            throw new NotImplementedException();
        }

        IList<string> IViewFolder.ListViews(string path)
        {
            return ViewSourceLoader.ListViews(path);
        }

        bool IViewFolder.HasView(string path)
        {
            return ViewSourceLoader.HasSource(path);
        }

        IViewFile IViewFolder.GetViewSource(string path)
        {
            return new ViewFile(ViewSourceLoader.GetViewSource(Path.ChangeExtension(path, ViewFileExtension)));
        }

        private class ViewFile : IViewFile
        {
            private readonly IViewSource _source;

            public ViewFile(IViewSource source)
            {
                _source = source;
            }

            public long LastModified
            {
                get { return _source.LastModified; }
            }

            public Stream OpenViewStream()
            {
                return _source.OpenViewStream();
            }
        }

        readonly Dictionary<string, Type> _cachedViewComponent = new Dictionary<string, Type>();

        ISparkExtension ISparkExtensionFactory.CreateExtension(ElementNode node)
        {
            var componentFactory = (IViewComponentFactory)serviceProvider.GetService(typeof(IViewComponentFactory));

            Type viewComponent;
            lock (_cachedViewComponent)
            {

                if (!_cachedViewComponent.TryGetValue(node.Name, out viewComponent))
                {
                    try
                    {
                        viewComponent = componentFactory.Registry.GetViewComponent(node.Name);
                        _cachedViewComponent.Add(node.Name, viewComponent);
                    }
                    catch
                    {
                        _cachedViewComponent.Add(node.Name, null);
                    }
                }
            }
            if (viewComponent != null)
                return new ViewComponentExtension(node);

            return null;
        }


        public Assembly Precompile(SparkBatchDescriptor batch)
        {
            return Engine.BatchCompilation(batch.OutputAssembly, CreateDescriptors(batch));
        }

        public IList<SparkViewDescriptor> CreateDescriptors(SparkBatchDescriptor batch)
        {
            var descriptors = new List<SparkViewDescriptor>();
            foreach (var entry in batch.Entries)
                descriptors.AddRange(CreateDescriptors(entry));
            return descriptors;
        }

        public IList<SparkViewDescriptor> CreateDescriptors(SparkBatchEntry entry)
        {
            var descriptors = new List<SparkViewDescriptor>();

            var metaDesc = _controllerDescriptorProvider.BuildDescriptor(entry.ControllerType);

            var controllerName = metaDesc.ControllerDescriptor.Name;
            var controllerPath = controllerName;
            if (!string.IsNullOrEmpty(metaDesc.ControllerDescriptor.Area))
                controllerPath = metaDesc.ControllerDescriptor.Area + "\\" + controllerName;

            var viewNames = new List<string>();
            var includeViews = entry.IncludeViews;
            if (includeViews.Count == 0)
                includeViews = new[] { "*" };

            foreach (var include in includeViews)
            {
                if (include.EndsWith("*"))
                {
                    foreach (var fileName in ViewSourceLoader.ListViews(controllerPath))
                    {
                        var potentialMatch = Path.GetFileNameWithoutExtension(fileName);
                        if (!TestMatch(potentialMatch, include))
                            continue;

                        var isExcluded = false;
                        foreach (var exclude in entry.ExcludeViews)
                        {
                            if (!TestMatch(potentialMatch, RemoveSuffix(exclude, ".spark")))
                                continue;

                            isExcluded = true;
                            break;
                        }
                        if (!isExcluded)
                            viewNames.Add(potentialMatch);
                    }
                }
                else
                {
                    // explicitly included views don't test for exclusion
                    viewNames.Add(RemoveSuffix(include, ".spark"));
                }
            }

            foreach (var viewName in viewNames)
            {
                var layoutNamesList = entry.LayoutNames;
                if (layoutNamesList.Count == 0)
                {
                    var action = metaDesc.Actions[viewName];
                    if (action != null)
                    {
                        var actionDesc = metaDesc.ActionDescriptors[action];
                        if (actionDesc != null && actionDesc.Layout != null)
                            layoutNamesList = new[] { actionDesc.Layout.LayoutNames };
                    }
                }
                if (layoutNamesList.Count == 0)
                {
                    if (metaDesc.Layout != null)
                        layoutNamesList = new[] { metaDesc.Layout.LayoutNames };                    
                }

                foreach (var layoutNames in layoutNamesList)
                {
                    descriptors.Add(CreateDescriptor(
                                        entry.ControllerType.Namespace,
                                        controllerPath,
                                        viewName,
                                        layoutNames));
                }
            }

            return descriptors;
        }

        private SparkViewDescriptor CreateDescriptor(
            string targetNamespace,
            string controllerPath,
            string viewName,
            IList<string> layouts)
        {
            var descriptor = new SparkViewDescriptor
            {
                TargetNamespace = targetNamespace
            };

            if (HasTemplate(controllerPath + "\\" + viewName + ".spark"))
            {
                descriptor.Templates.Add(controllerPath + "\\" + viewName + ".spark");
            }
            else
            {
                throw new CompilerException(string.Format("Unable to find templates {0}\\{1}.spark", controllerPath, viewName));
            }

            foreach (var layoutName in layouts ?? new string[0])
            {
                if (HasTemplate("Layouts\\" + layoutName))
                {
                    descriptor.Templates.Add(Path.ChangeExtension("Layouts\\" + layoutName, ViewFileExtension));
                }
                else if (HasTemplate("Shared\\" + layoutName))
                {
                    descriptor.Templates.Add(Path.ChangeExtension("Shared\\" + layoutName, ViewFileExtension));
                }
                else
                {
                    throw new CompilerException(string.Format(
                                                    "Unable to find templates layouts\\{0}.spark or shared\\{0}.spark",
                                                    layoutName));
                }
            }

            return descriptor;
        }

        static bool TestMatch(string potentialMatch, string pattern)
        {
            if (!pattern.EndsWith("*"))
            {
                return string.Equals(potentialMatch, pattern, StringComparison.InvariantCultureIgnoreCase);
            }

            // raw wildcard matches anything that's not a partial
            if (pattern == "*")
            {
                return !potentialMatch.StartsWith("_");
            }

            // otherwise the only thing that's supported is "starts with"
            return potentialMatch.StartsWith(pattern.Substring(0, pattern.Length - 1),
                                             StringComparison.InvariantCultureIgnoreCase);
        }

        static string RemoveSuffix(string value, string suffix)
        {
            if (value.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase))
                return value.Substring(0, value.Length - suffix.Length);
            return value;
        }

    }
}
