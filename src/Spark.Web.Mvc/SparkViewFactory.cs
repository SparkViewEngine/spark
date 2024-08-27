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
using System.Web.Mvc;
using Spark.Descriptors;
using Spark.FileSystem;
using Spark.Web.Mvc.Wrappers;

namespace Spark.Web.Mvc
{
    public class SparkWebPrecompiler : SparkPrecompiler
    {
        private readonly IDescriptorBuilder DescriptorBuilder;

        public SparkWebPrecompiler(ISparkViewEngine engine, IDescriptorBuilder descriptorBuilder) : base(engine, descriptorBuilder)
        {
            this.DescriptorBuilder = descriptorBuilder;
        }

        [Obsolete("Is this used apart from in the unit tests?")]
        public SparkViewDescriptor CreateDescriptor(
            ControllerContext controllerContext,
            string viewName,
            string masterName,
            bool findDefaultMaster,
            ICollection<string> searchedLocations)
        {
            var targetNamespace = controllerContext.Controller.GetType().Namespace;

            var controllerName = controllerContext.RouteData.GetRequiredString("controller");

            var routeDataWrapper = new SparkRouteData(controllerContext.RouteData.Values);

            return DescriptorBuilder.BuildDescriptor(
                new BuildDescriptorParams(
                    targetNamespace,
                    controllerName,
                    viewName,
                    masterName,
                    findDefaultMaster,
                    DescriptorBuilder.GetExtraParameters(routeDataWrapper)),
                searchedLocations);
        }
    }

    public class SparkViewFactory : IViewEngine, IViewFolderContainer
    {
        public ISparkSettings Settings { get; protected set; }
        public ISparkViewEngine Engine { get; protected set; }
        public IDescriptorBuilder DescriptorBuilder { get; protected set; }
        public IResourcePathManager ResourcePathManager { get; protected set; }
        public ICacheService CacheService { get; protected set; }
        public ISparkPrecompiler Precompiler { get; protected set; }

        private readonly Dictionary<BuildDescriptorParams, ISparkViewEntry> cache;
        private readonly ViewEngineResult cacheMissResult;

        public SparkViewFactory(
            ISparkSettings settings, 
            ISparkViewEngine viewEngine,
            IDescriptorBuilder descriptorBuilder,
            IResourcePathManager resourcePathManager,
            ICacheService cacheService,
            ISparkPrecompiler precompiler)
        {
            this.Settings = settings;

            if (string.IsNullOrEmpty(settings.BaseClassTypeName) && settings is SparkSettings sparkSettings)
            {
                sparkSettings.SetBaseClass<SparkView>();
            }

            this.Engine = viewEngine;
            this.DescriptorBuilder = descriptorBuilder;
            this.ResourcePathManager = resourcePathManager;
            this.CacheService = cacheService;
            this.Precompiler = precompiler;

            this.cache = new Dictionary<BuildDescriptorParams, ISparkViewEntry>();
            this.cacheMissResult = new ViewEngineResult(Array.Empty<string>());
        }

        public IViewActivatorFactory ViewActivatorFactory => this.Engine.ViewActivatorFactory;

        public virtual ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName)
        {
            return this.FindViewInternal(controllerContext, viewName, masterName, true, false);
        }

        public virtual ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return this.FindViewInternal(controllerContext, viewName, masterName, true, useCache);
        }

        public virtual ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName)
        {
            return this.FindViewInternal(controllerContext, partialViewName, null /*masterName*/, false, false);
        }

        public virtual ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return this.FindViewInternal(controllerContext, partialViewName, null /*masterName*/, false, useCache);
        }

        public virtual void ReleaseView(ControllerContext controllerContext, IView view)
        {
            if (view is ISparkView sparkView)
            {
                this.Engine.ReleaseInstance(sparkView);
            }
        }

        private ViewEngineResult FindViewInternal(ControllerContext controllerContext, string viewName, string masterName, bool findDefaultMaster, bool useCache)
        {
            var searchedLocations = new List<string>();
            var targetNamespace = controllerContext.Controller.GetType().Namespace;

            var controllerName = controllerContext.RouteData.GetRequiredString("controller");

            var routeDataWrapper = new SparkRouteData(controllerContext.RouteData.Values);

            var descriptorParams = new BuildDescriptorParams(
                targetNamespace,
                controllerName,
                viewName,
                masterName,
                findDefaultMaster,
                this.DescriptorBuilder.GetExtraParameters(routeDataWrapper));

            ISparkViewEntry entry;
            if (useCache)
            {
                if (this.TryGetCacheValue(descriptorParams, out entry) && entry.IsCurrent())
                {
                    return this.BuildResult(entry);
                }

                return this.cacheMissResult;
            }

            var descriptor = this.DescriptorBuilder.BuildDescriptor(
                descriptorParams,
                searchedLocations);

            if (descriptor == null)
            {
                return new ViewEngineResult(searchedLocations);
            }

            entry = this.Engine.CreateEntry(descriptor);

            this.SetCacheValue(descriptorParams, entry);

            return this.BuildResult(entry);
        }

        private bool TryGetCacheValue(BuildDescriptorParams descriptorParams, out ISparkViewEntry entry)
        {
            lock (this.cache) return this.cache.TryGetValue(descriptorParams, out entry);
        }

        private void SetCacheValue(BuildDescriptorParams descriptorParams, ISparkViewEntry entry)
        {
            lock (this.cache) this.cache[descriptorParams] = entry;
        }

        private ViewEngineResult BuildResult(ISparkViewEntry entry)
        {
            var view = (IView)entry.CreateInstance();
            
            if (view is SparkView sparkView)
            {
                sparkView.ResourcePathManager = this.ResourcePathManager;
                sparkView.CacheService = this.CacheService;
            }

            return new ViewEngineResult(view, this);
        }

        [Obsolete("Does not seem to be used?")]
        public SparkViewDescriptor CreateDescriptor(
            ControllerContext controllerContext,
            string viewName,
            string masterName,
            bool findDefaultMaster,
            ICollection<string> searchedLocations)
        {
            var targetNamespace = controllerContext.Controller.GetType().Namespace;

            var controllerName = controllerContext.RouteData.GetRequiredString("controller");

            var routeDataWrapper = new SparkRouteData(controllerContext.RouteData.Values);

            return DescriptorBuilder.BuildDescriptor(
                new BuildDescriptorParams(
                    targetNamespace,
                    controllerName,
                    viewName,
                    masterName,
                    findDefaultMaster,
                    DescriptorBuilder.GetExtraParameters(routeDataWrapper)),
                searchedLocations);
        }
        
        public Assembly Precompile(SparkBatchDescriptor batch)
        {
            return this.Precompiler.Precompile(batch);
        }

        #region IViewEngine Members

        ViewEngineResult IViewEngine.FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return this.FindPartialView(controllerContext, partialViewName, useCache);
        }

        ViewEngineResult IViewEngine.FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return this.FindView(controllerContext, viewName, masterName, useCache);
        }

        void IViewEngine.ReleaseView(ControllerContext controllerContext, IView view)
        {
            this.ReleaseView(controllerContext, view);
        }

        #endregion

        #region IViewFolderContainer Members

        IViewFolder IViewFolderContainer.ViewFolder
        {
            get => this.Engine.ViewFolder;
            set => this.Engine.ViewFolder = value;
        }

        #endregion
    }
}