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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Web.Mvc.Wrappers;

namespace Spark.Web.Mvc
{
    public class SparkViewFactory : IViewEngine, IViewFolderContainer, ISparkServiceInitialize
    {
        private ISparkViewEngine _engine;
        private IDescriptorBuilder _descriptorBuilder;
        private ICacheServiceProvider _cacheServiceProvider;


        public SparkViewFactory()
            : this(null)
        {
        }

        public SparkViewFactory(ISparkSettings settings)
        {
            Settings = settings ?? (ISparkSettings)ConfigurationManager.GetSection("spark") ?? new SparkSettings();
        }


        public virtual void Initialize(ISparkServiceContainer container)
        {
            Settings = container.GetService<ISparkSettings>();
            Engine = container.GetService<ISparkViewEngine>();
            DescriptorBuilder = container.GetService<IDescriptorBuilder>();
            CacheServiceProvider = container.GetService<ICacheServiceProvider>();
        }

        public ISparkSettings Settings { get; set; }

        public ISparkViewEngine Engine
        {
            get
            {
                if (_engine == null)
                    SetEngine(new SparkViewEngine(Settings));

                return _engine;
            }
            set
            {
                SetEngine(value);
            }
        }

        public void SetEngine(ISparkViewEngine engine)
        {
            _descriptorBuilder = null;
            _engine = engine;
            if (_engine != null)
            {
                _engine.DefaultPageBaseType = typeof(SparkView).FullName;
            }
        }

        public IViewActivatorFactory ViewActivatorFactory
        {
            get { return Engine.ViewActivatorFactory; }
            set { Engine.ViewActivatorFactory = value; }
        }

        public IViewFolder ViewFolder
        {
            get { return Engine.ViewFolder; }
            set { Engine.ViewFolder = value; }
        }

        public IDescriptorBuilder DescriptorBuilder
        {
            get
            {
                return _descriptorBuilder ??
                       Interlocked.CompareExchange(ref _descriptorBuilder, new DefaultDescriptorBuilder(Engine), null) ??
                       _descriptorBuilder;
            }
            set { _descriptorBuilder = value; }
        }

        public ICacheServiceProvider CacheServiceProvider
        {
            get
            {
                return _cacheServiceProvider ??
                       Interlocked.CompareExchange(ref _cacheServiceProvider, new DefaultCacheServiceProvider(), null) ??
                       _cacheServiceProvider;
            }
            set { _cacheServiceProvider = value; }
        }

        public virtual ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName)
        {
            return FindViewInternal(controllerContext, viewName, masterName, true, false);
        }

        public virtual ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return FindViewInternal(controllerContext, viewName, masterName, true, useCache);
        }

        public virtual ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName)
        {
            return FindViewInternal(controllerContext, partialViewName, null /*masterName*/, false, false);
        }

        public virtual ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return FindViewInternal(controllerContext, partialViewName, null /*masterName*/, false, useCache);
        }

        public virtual void ReleaseView(ControllerContext controllerContext, IView view)
        {
            var sparkView = view as ISparkView;
            if (sparkView != null)
                Engine.ReleaseInstance(sparkView);
        }

        private readonly Dictionary<BuildDescriptorParams, ISparkViewEntry> _cache =
            new Dictionary<BuildDescriptorParams, ISparkViewEntry>();

        private readonly ViewEngineResult _cacheMissResult = new ViewEngineResult(new string[0]);

        private ViewEngineResult FindViewInternal(ControllerContext controllerContext, string viewName, string masterName, bool findDefaultMaster, bool useCache)
        {
            var searchedLocations = new List<string>();
            var targetNamespace = controllerContext.Controller.GetType().Namespace;

            var controllerName = controllerContext.RouteData.GetRequiredString("controller");

            var descriptorParams = new BuildDescriptorParams(
                targetNamespace,
                controllerName,
                viewName,
                masterName,
                findDefaultMaster,
                DescriptorBuilder.GetExtraParameters(controllerContext));

            ISparkViewEntry entry;
            if (useCache)
            {
                if (TryGetCacheValue(descriptorParams, out entry) && entry.IsCurrent())
                {
                    return BuildResult(controllerContext.RequestContext, entry);
                }
                return _cacheMissResult;
            }

            var descriptor = DescriptorBuilder.BuildDescriptor(
                descriptorParams,
                searchedLocations);

            if (descriptor == null)
                return new ViewEngineResult(searchedLocations);

            entry = Engine.CreateEntry(descriptor);
            SetCacheValue(descriptorParams, entry);
            return BuildResult(controllerContext.RequestContext, entry);
        }

        private bool TryGetCacheValue(BuildDescriptorParams descriptorParams, out ISparkViewEntry entry)
        {
            lock (_cache) return _cache.TryGetValue(descriptorParams, out entry);
        }

        private void SetCacheValue(BuildDescriptorParams descriptorParams, ISparkViewEntry entry)
        {
            lock (_cache) _cache[descriptorParams] = entry;
        }


        private ViewEngineResult BuildResult(RequestContext requestContext, ISparkViewEntry entry)
        {
            var view = (IView)entry.CreateInstance();
            if (view is SparkView)
            {
                var sparkView = (SparkView)view;
                sparkView.ResourcePathManager = Engine.ResourcePathManager;
                sparkView.CacheService = CacheServiceProvider.GetCacheService(requestContext);
            }
            return new ViewEngineResult(view, this);
        }

        public SparkViewDescriptor CreateDescriptor(
            ControllerContext controllerContext,
            string viewName,
            string masterName,
            bool findDefaultMaster,
            ICollection<string> searchedLocations)
        {
            var targetNamespace = controllerContext.Controller.GetType().Namespace;

            var controllerName = controllerContext.RouteData.GetRequiredString("controller");

            return DescriptorBuilder.BuildDescriptor(
                new BuildDescriptorParams(
                    targetNamespace,
                    controllerName,
                    viewName,
                    masterName,
                    findDefaultMaster,
                    DescriptorBuilder.GetExtraParameters(controllerContext)),
                searchedLocations);
        }

        public SparkViewDescriptor CreateDescriptor(string targetNamespace, string controllerName, string viewName,
                                                    string masterName, bool findDefaultMaster)
        {
            var searchedLocations = new List<string>();
            var descriptor = DescriptorBuilder.BuildDescriptor(
                new BuildDescriptorParams(
                    targetNamespace /*areaName*/,
                    controllerName,
                    viewName,
                    masterName,
                    findDefaultMaster, null),
                searchedLocations);

            if (descriptor == null)
            {
                throw new CompilerException("Unable to find templates at " +
                                            string.Join(", ", searchedLocations.ToArray()));
            }
            return descriptor;
        }


        public Assembly Precompile(SparkBatchDescriptor batch)
        {
            return Engine.BatchCompilation(batch.OutputAssembly, CreateDescriptors(batch));
        }

        public List<SparkViewDescriptor> CreateDescriptors(SparkBatchDescriptor batch)
        {
            var descriptors = new List<SparkViewDescriptor>();
            foreach (var entry in batch.Entries)
                descriptors.AddRange(CreateDescriptors(entry));
            return descriptors;
        }

        public IList<SparkViewDescriptor> CreateDescriptors(SparkBatchEntry entry)
        {
            var descriptors = new List<SparkViewDescriptor>();

            var controllerName = RemoveSuffix(entry.ControllerType.Name, "Controller");

            var viewNames = new List<string>();
            var includeViews = entry.IncludeViews;
            if (includeViews.Count == 0)
                includeViews = new[] { "*" };

            foreach (var include in includeViews)
            {
                if (include.EndsWith("*"))
                {
                    foreach (var fileName in ViewFolder.ListViews(controllerName))
                    {
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
                if (entry.LayoutNames.Count == 0)
                {
                    descriptors.Add(CreateDescriptor(
                                        entry.ControllerType.Namespace,
                                        controllerName,
                                        viewName,
                                        null /*masterName*/,
                                        true));
                }
                else
                {
                    foreach (var masterName in entry.LayoutNames)
                    {
                        descriptors.Add(CreateDescriptor(
                                            entry.ControllerType.Namespace,
                                            controllerName,
                                            viewName,
                                            string.Join(" ", masterName.ToArray()),
                                            false));
                    }
                }
            }

            return descriptors;
        }

        private static bool TestMatch(string potentialMatch, string pattern)
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

        private static string RemoveSuffix(string value, string suffix)
        {
            if (value.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase))
                return value.Substring(0, value.Length - suffix.Length);
            return value;
        }



        #region IViewEngine Members

        ViewEngineResult IViewEngine.FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return FindPartialView(controllerContext, partialViewName, useCache);
        }

        ViewEngineResult IViewEngine.FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return FindView(controllerContext, viewName, masterName, useCache);
        }

        void IViewEngine.ReleaseView(ControllerContext controllerContext, IView view)
        {
            ReleaseView(controllerContext, view);
        }

        #endregion


        #region ISparkServiceInitialize Members

        void ISparkServiceInitialize.Initialize(ISparkServiceContainer container)
        {
            Initialize(container);
        }

        #endregion


        #region IViewFolderContainer Members

        IViewFolder IViewFolderContainer.ViewFolder
        {
            get { return ViewFolder; }
            set { ViewFolder = value; }
        }

        #endregion
    }
}