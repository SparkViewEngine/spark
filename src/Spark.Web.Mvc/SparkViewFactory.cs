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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Web.Mvc.Wrappers;

namespace Spark.Web.Mvc
{
    public class SparkViewFactory : IViewEngine, IViewFolderContainer, ISparkServiceInitialize
    {
        private ISparkViewEngine _engine;


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


        public virtual ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName)
        {
            return FindViewInternal(controllerContext, viewName, masterName, true);
        }

        public virtual ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName)
        {
            return FindViewInternal(controllerContext, partialViewName, null /*masterName*/, false);
        }

        public virtual void ReleaseView(ControllerContext controllerContext, IView view)
        {
            Engine.ReleaseInstance((ISparkView)view);
        }

        private string GetAreaName(ControllerContext controllerContext)
        {
            object areaName;
            return controllerContext.RouteData.Values.TryGetValue("area", out areaName)
                       ? Convert.ToString(areaName)
                       : null;
        }

        private ViewEngineResult FindViewInternal(ControllerContext controllerContext, string viewName, string masterName, bool findDefaultMaster)
        {
            var searchedLocations = new List<string>();
            var descriptor = CreateDescriptor(controllerContext, viewName, masterName, findDefaultMaster,
                                              searchedLocations);

            if (descriptor == null)
                return new ViewEngineResult(searchedLocations);

            var entry = Engine.CreateEntry(descriptor);
            var view = (IView)entry.CreateInstance();
            if (view is SparkView)
            {
                ((SparkView)view).ResourcePathManager = Engine.ResourcePathManager;
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
            var areaName = GetAreaName(controllerContext);
            var controllerName = controllerContext.RouteData.GetRequiredString("controller");

            return CreateDescriptorInternal(
                targetNamespace, 
                areaName, 
                controllerName, 
                viewName, 
                masterName, 
                findDefaultMaster,
                searchedLocations);
        }

        public SparkViewDescriptor CreateDescriptor(string targetNamespace, string controllerName, string viewName,
                                                    string masterName, bool findDefaultMaster)
        {
            var searchedLocations = new List<string>();
            var descriptor = CreateDescriptorInternal(targetNamespace, null, controllerName, viewName, masterName,
                                                      findDefaultMaster,
                                                      searchedLocations);
            if (descriptor == null)
            {
                throw new CompilerException("Unable to find templates at " +
                                            string.Join(", ", searchedLocations.ToArray()));
            }
            return descriptor;
        }

        internal SparkViewDescriptor CreateDescriptorInternal(string targetNamespace, string areaName, string controllerName, string viewName, string masterName, bool findDefaultMaster, ICollection<string> searchedLocations)
        {
            var descriptor = new SparkViewDescriptor
                                 {
                                     TargetNamespace = targetNamespace
                                 };

            if (!LocatePotentialTemplate(
                PotentialViewLocations(areaName, controllerName, viewName),
                descriptor.Templates,
                searchedLocations))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(masterName))
            {
                if (!LocatePotentialTemplate(
                    PotentialMasterLocations(areaName, masterName),
                    descriptor.Templates,
                    searchedLocations))
                {
                    return null;
                }
            }
            else if (findDefaultMaster)
            {
                LocatePotentialTemplate(
                    PotentialDefaultMasterLocations(areaName, controllerName),
                    descriptor.Templates,
                    null);
            }

            return descriptor;
        }

        private bool LocatePotentialTemplate(
            IEnumerable<string> potentialTemplates,
            ICollection<string> descriptorTemplates,
            ICollection<string> searchedLocations)
        {
            var template = potentialTemplates.FirstOrDefault(t => ViewFolder.HasView(t));
            if (template != null)
            {
                descriptorTemplates.Add(template);
                return true;
            }
            if (searchedLocations != null)
            {
                foreach (var potentialTemplate in potentialTemplates)
                    searchedLocations.Add(potentialTemplate);
            }
            return false;
        }

        protected virtual IEnumerable<string> PotentialViewLocations(string areaName, string controllerName, string viewName)
        {
            if (string.IsNullOrEmpty(areaName))
            {
                return new[]
                           {
                               controllerName + "\\" + viewName + ".spark",
                               "Shared\\" + viewName + ".spark"
                           };

            }

            return new[]
                       {
                           areaName + "\\" + controllerName + "\\" + viewName + ".spark",
                           controllerName + "\\" + viewName + ".spark",
                           "Shared\\" + viewName + ".spark"
                       };
        }

        protected virtual IEnumerable<string> PotentialMasterLocations(string areaName, string masterName)
        {
            if (string.IsNullOrEmpty(areaName))
            {
                return new[]
                       {
                           "Layouts\\" + masterName + ".spark",
                           "Shared\\" + masterName + ".spark"
                       };
            }
            return new[]
                           {
                               areaName + "\\Layouts\\" + masterName + ".spark",
                               areaName + "\\Shared\\" + masterName + ".spark",
                               "Layouts\\" + masterName + ".spark",
                               "Shared\\" + masterName + ".spark"
                           };
        }

        protected virtual IEnumerable<string> PotentialDefaultMasterLocations(string areaName, string controllerName)
        {
            if (string.IsNullOrEmpty(areaName))
            {
                return new[]
                           {
                               "Layouts\\" + controllerName + ".spark",
                               "Shared\\" + controllerName + ".spark",
                               "Layouts\\Application.spark",
                               "Shared\\Application.spark"
                           };

            }
            return new[]
                       {
                           areaName + "\\Layouts\\" + controllerName + ".spark",
                           areaName + "\\Shared\\" + controllerName + ".spark",
                           areaName + "\\Layouts\\Application.spark",
                           areaName + "\\Shared\\Application.spark",
                           "Layouts\\" + controllerName + ".spark",
                           "Shared\\" + controllerName + ".spark",
                           "Layouts\\Application.spark",
                           "Shared\\Application.spark"
                       };
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
            return FindPartialView(controllerContext, partialViewName);
        }

        ViewEngineResult IViewEngine.FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return FindView(controllerContext, viewName, masterName);
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