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
using System.Collections;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using Castle.Core.Logging;
using Castle.MonoRail.Framework.Descriptors;
using Castle.MonoRail.Framework.Resources;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.Views.Spark.Wrappers;
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

    public class SparkViewFactory : ViewEngineBase, IViewSourceLoaderContainer
    {
        private IControllerDescriptorProvider _controllerDescriptorProvider;
        private IViewActivatorFactory _viewActivatorFactory;

        public override void Service(IServiceProvider provider)
        {
            base.Service(provider);

            _controllerDescriptorProvider = (IControllerDescriptorProvider)provider.GetService(typeof(IControllerDescriptorProvider));
            _viewActivatorFactory = (IViewActivatorFactory)provider.GetService(typeof(IViewActivatorFactory));
            _cacheServiceProvider = (ICacheServiceProvider)provider.GetService(typeof(ICacheServiceProvider));

            SetEngine((ISparkViewEngine)provider.GetService(typeof(ISparkViewEngine)));
        }

        private ISparkViewEngine _engine;
        public ISparkViewEngine Engine
        {
            get
            {
                if (_engine == null)
                    SetEngine(new SparkViewEngine());
                return _engine;
            }
            set
            {
                SetEngine(value);
            }
        }

        private void SetEngine(ISparkViewEngine engine)
        {
            _engine = engine;
            if (_engine == null)
                return;

            _engine.ViewFolder = new ViewSourceLoaderWrapper(this);
            _engine.ExtensionFactory = new ViewComponentExtensionFactory(serviceProvider);
            _engine.DefaultPageBaseType = typeof(SparkView).FullName;

            if (_viewActivatorFactory != null)
                _engine.ViewActivatorFactory = _viewActivatorFactory;
        }

        private ICacheServiceProvider _cacheServiceProvider;
        public ICacheServiceProvider CacheServiceProvider
        {
            get
            {
                if (_cacheServiceProvider == null)
                    _cacheServiceProvider = new HybridCacheServiceProvider();
                return _cacheServiceProvider;
            }
            set
            {
                _cacheServiceProvider = value;
            }
        }

        IViewSourceLoader IViewSourceLoaderContainer.ViewSourceLoader
        {
            get { return ViewSourceLoader; }
            set { ViewSourceLoader = value; }
        }

        public override string ViewFileExtension
        {
            get { return "spark"; }
        }

        public override bool HasTemplate(string templateName)
        {
            //return base.HasTemplate(Path.ChangeExtension(templateName, ViewFileExtension));
            return Engine.ViewFolder.HasView(Path.ChangeExtension(templateName, ViewFileExtension));
        }

        private string LayoutPath(string layoutName)
        {
            if (HasTemplate(layoutName))
                return layoutName;

            if (HasTemplate(string.Format("Layouts{0}{1}", Path.DirectorySeparatorChar, layoutName)))
                return string.Format("Layouts{0}{1}", Path.DirectorySeparatorChar, layoutName);

            if (HasTemplate(string.Format("Shared{0}{1}", Path.DirectorySeparatorChar, layoutName)))
                return string.Format("Shared{0}{1}", Path.DirectorySeparatorChar, layoutName);

            throw new CompilerException(string.Format(
                                            "Unable to find templates {0} or layouts{1}{0} or shared{1}{0}",
                                            layoutName, Path.DirectorySeparatorChar));
        }

        public override void Process(string templateName, TextWriter output, IEngineContext context, IController controller,
                                     IControllerContext controllerContext)
        {
            var descriptor = new SparkViewDescriptor();
            if (controller != null)
                descriptor.TargetNamespace = controller.GetType().Namespace;

            descriptor.Templates.Add(Path.ChangeExtension(templateName, ViewFileExtension));

            foreach (var layoutName in (controllerContext.LayoutNames ?? new string[0]).Reverse())
            {
                descriptor.Templates.Add(Path.ChangeExtension(LayoutPath(layoutName), ViewFileExtension));
            }

            if (controllerContext.ControllerDescriptor != null)
            {
                foreach (var helper in controllerContext.ControllerDescriptor.Helpers)
                {
                    var typeName = helper.HelperType.FullName;
                    var propertyName = helper.Name ?? helper.HelperType.Name;
                    descriptor.AddAccessor(
                        typeName + " " + propertyName,
                        "Helper<" + typeName + ">(\"" + propertyName + "\")");
                }
            }

            var entry = Engine.CreateEntry(descriptor);
            var view = (SparkView)entry.CreateInstance();
            view.Contextualize(context, controllerContext, this, null);
            if (view.Logger == null || view.Logger == NullLogger.Instance)
                view.Logger = Logger;
            view.RenderView(output);

            // proactively dispose named content. pools spoolwriter pages. avoids finalizers.
            foreach (var writer in view.Content.Values)
                writer.Close();

            view.Content.Clear();

            entry.ReleaseInstance(view);
        }

        public override void Process(string templateName, string layoutName, TextWriter output,
                                     IDictionary<string, object> parameters)
        {
            IEngineContext engineContext = null;
            if (HttpContext.Current != null)
            {
                engineContext = (IEngineContext)HttpContext.Current.Items["currentmrengineinstance"];
            }

            var controllerContext = new BasicControllerContext
                                        {
                                            SelectedViewName = templateName
                                        };

            if (!string.IsNullOrEmpty(layoutName))
                controllerContext.LayoutNames = new[] { layoutName };

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                    controllerContext.PropertyBag[parameter.Key] = parameter.Value;
            }

            if (engineContext != null)
            {
                var controller = engineContext.CurrentController as Controller;
                if (controller != null)
                {
                    foreach (string key in controller.Helpers.Keys)
                        controllerContext.Helpers.Add(key, controller.Helpers[key]);
                }
            }

            Process(templateName, output, engineContext, null, controllerContext);
        }

        class BasicControllerContext : IControllerContext
        {
            public BasicControllerContext()
            {
                CustomActionParameters = new Dictionary<string, object>();
                PropertyBag = new Hashtable();
                Helpers = new HelperDictionary();
                Resources = new Dictionary<string, IResource>();
            }

            public IDictionary<string, object> CustomActionParameters { get; set; }
            public IDictionary PropertyBag { get; set; }
            public HelperDictionary Helpers { get; set; }
            public string Name { get; set; }
            public string AreaName { get; set; }
            public string[] LayoutNames { get; set; }
            public string Action { get; set; }
            public string SelectedViewName { get; set; }
            public string ViewFolder { get; set; }
            public IDictionary<string, IResource> Resources { get; private set; }
            public IDictionary<string, IDynamicAction> DynamicActions { get; private set; }
            public ControllerMetaDescriptor ControllerDescriptor { get; set; }
            public RouteMatch RouteMatch { get; set; }
            public AsyncInvocationInformation Async { get; set; }
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
                controllerPath = string.Format("{0}{1}{2}", metaDesc.ControllerDescriptor.Area, Path.DirectorySeparatorChar, controllerName);

            var viewNames = new List<string>();
            var includeViews = entry.IncludeViews;
            if (includeViews.Count == 0)
                includeViews = new[] { "*" };

            var accessors = new List<SparkViewDescriptor.Accessor>();
            foreach (var helper in metaDesc.Helpers)
            {
                var typeName = helper.HelperType.FullName;
                var propertyName = helper.Name ?? helper.HelperType.Name;
                accessors.Add(new SparkViewDescriptor.Accessor
                                  {
                                      Property = typeName + " " + propertyName,
                                      GetValue = "Helper<" + typeName + ">(\"" + propertyName + "\")"
                                  });
            }

            foreach (var include in includeViews)
            {
                if (include.EndsWith("*"))
                {
                    foreach (var fileName in ViewSourceLoader.ListViews(controllerPath))
                    {
                        // ignore files which are not spark extension
                        if (!string.Equals(Path.GetExtension(fileName), ".spark", StringComparison.InvariantCultureIgnoreCase))
                            continue;

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
                    else
                        layoutNamesList = new[] { new string[0] };
                }

                foreach (var layoutNames in layoutNamesList)
                {
                    descriptors.Add(CreateDescriptor(
                                        entry.ControllerType.Namespace,
                                        controllerPath,
                                        viewName,
                                        layoutNames,
                                        accessors));
                }
            }

            return descriptors;
        }


        private SparkViewDescriptor CreateDescriptor(
            string targetNamespace,
            string controllerPath,
            string viewName,
            IList<string> layouts,
            IEnumerable<SparkViewDescriptor.Accessor> accessors)
        {
            var descriptor = new SparkViewDescriptor
            {
                TargetNamespace = targetNamespace
            };

            if (HasTemplate(string.Format("{0}{1}{2}.spark", controllerPath, Path.DirectorySeparatorChar, viewName)))
            {
                descriptor.Templates.Add(string.Format("{0}{1}{2}.spark", controllerPath, Path.DirectorySeparatorChar, viewName));
            }
            else
            {
                throw new CompilerException(string.Format("Unable to find templates {0}{1}{2}.spark", controllerPath, Path.DirectorySeparatorChar, viewName));
            }

            foreach (var layoutName in (layouts ?? new string[0]).Reverse())
            {
                if (HasTemplate(string.Format("Layouts{0}{1}", Path.DirectorySeparatorChar, layoutName)))
                {
                    descriptor.Templates.Add(Path.ChangeExtension(string.Format("Layouts{0}{1}", Path.DirectorySeparatorChar, layoutName), ViewFileExtension));
                }
                else if (HasTemplate(string.Format("Shared{0}{1}", Path.DirectorySeparatorChar, layoutName)))
                {
                    descriptor.Templates.Add(Path.ChangeExtension(string.Format("Shared{0}{1}", Path.DirectorySeparatorChar, layoutName), ViewFileExtension));
                }
                else
                {
                    throw new CompilerException(string.Format(
                                                    "Unable to find templates layouts{0}{1}.spark or shared{0}{1}.spark",
                                                    Path.DirectorySeparatorChar, layoutName));
                }
            }

            foreach (var accessor in accessors)
            {
                descriptor.AddAccessor(accessor.Property, accessor.GetValue);
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
