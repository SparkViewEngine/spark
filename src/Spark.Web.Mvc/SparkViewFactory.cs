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
    public class SparkViewFactory : IViewEngine, IViewFolderContainer
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
                _engine.ViewFolder = new ViewFolderWrapper(this);
                _engine.DefaultPageBaseType = typeof(SparkView).FullName;
            }
        }

        public IViewActivatorFactory ViewActivatorFactory
        {
            get { return Engine.ViewActivatorFactory; }
            set { Engine.ViewActivatorFactory = value; }
        }

        private IViewFolder _viewFolder;
        public IViewFolder ViewFolder
        {
            get
            {
                if (_viewFolder == null)
                    _viewFolder = CreateDefaultViewFolder();

                return _viewFolder;
            }
            set { _viewFolder = value; }
        }

        static IViewFolder CreateDefaultViewFolder()
        {
            string path;
            if (HttpContext.Current != null)
            {
                path = HttpContext.Current.Server.MapPath("~/Views");
            }
            else
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Views");
            }
            return new FileSystemViewFolder(path);
        }
        #region IViewEngine Members

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName)
        {
            return FindViewInternal(controllerContext, viewName, masterName, true);
        }

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName)
        {
            return FindViewInternal(controllerContext, partialViewName, null /*masterName*/, false);
        }

        #endregion

        private ViewEngineResult FindViewInternal(ControllerContext controllerContext, string viewName,
                                                  string masterName, bool findDefaultMaster)
        {
            var controllerName = controllerContext.RouteData.GetRequiredString("controller");
            var targetNamespace = controllerContext.Controller.GetType().Namespace;
            var searchedLocations = new List<string>();
            var descriptor = CreateDescriptorInternal(targetNamespace, controllerName, viewName, masterName,
                                                      findDefaultMaster, searchedLocations);
            if (descriptor == null)
                return new ViewEngineResult(searchedLocations);

            var entry = Engine.CreateEntry(descriptor);
            var view = (IView)entry.CreateInstance();
            return new ViewEngineResult(view);
        }

        public SparkViewDescriptor CreateDescriptor(ControllerContext controllerContext, string viewName,
                                                    string masterName, bool findDefaultMaster)
        {
            var controllerName = controllerContext.RouteData.GetRequiredString("controller");
            var targetNamespace = controllerContext.Controller.GetType().Namespace;

            return CreateDescriptorInternal(targetNamespace, controllerName, viewName, masterName, findDefaultMaster,
                                            null);
        }

        public SparkViewDescriptor CreateDescriptor(string targetNamespace, string controllerName, string viewName,
                                                    string masterName, bool findDefaultMaster)
        {
            var searchedLocations = new List<string>();
            var descriptor = CreateDescriptorInternal(targetNamespace, controllerName, viewName, masterName,
                                                      findDefaultMaster,
                                                      searchedLocations);
            if (descriptor == null)
            {
                throw new CompilerException("Unable to find templates at " +
                                            string.Join(", ", searchedLocations.ToArray()));
            }
            return descriptor;
        }

        private SparkViewDescriptor CreateDescriptorInternal(string targetNamespace, string controllerName,
                                                             string viewName, string masterName, bool findDefaultMaster,
                                                             ICollection<string> searchedLocations)
        {
            var descriptor = new SparkViewDescriptor
                                 {
                                     TargetNamespace = targetNamespace
                                 };

            if (ViewFolder.HasView(controllerName + "\\" + viewName + ".spark"))
            {
                descriptor.Templates.Add(controllerName + "\\" + viewName + ".spark");
            }
            else if (ViewFolder.HasView("Shared\\" + viewName + ".spark"))
            {
                descriptor.Templates.Add("Shared\\" + viewName + ".spark");
            }
            else
            {
                searchedLocations.Add(controllerName + "\\" + viewName + ".spark");
                searchedLocations.Add("Shared\\" + viewName + ".spark");
                return null;
            }

            if (!string.IsNullOrEmpty(masterName))
            {
                if (ViewFolder.HasView("Layouts\\" + masterName + ".spark"))
                {
                    descriptor.Templates.Add("Layouts\\" + masterName + ".spark");
                }
                else if (ViewFolder.HasView("Shared\\" + masterName + ".spark"))
                {
                    descriptor.Templates.Add("Shared\\" + masterName + ".spark");
                }
                else
                {
                    searchedLocations.Add("Layouts\\" + masterName + ".spark");
                    searchedLocations.Add("Shared\\" + masterName + ".spark");
                    return null;
                }
            }
            else if (findDefaultMaster)
            {
                if (ViewFolder.HasView("Layouts\\" + controllerName + ".spark"))
                {
                    descriptor.Templates.Add("Layouts\\" + controllerName + ".spark");
                }
                else if (ViewFolder.HasView("Shared\\" + controllerName + ".spark"))
                {
                    descriptor.Templates.Add("Shared\\" + controllerName + ".spark");
                }
                else if (ViewFolder.HasView("Layouts\\Application.spark"))
                {
                    descriptor.Templates.Add("Layouts\\Application.spark");
                }
                else if (ViewFolder.HasView("Shared\\Application.spark"))
                {
                    descriptor.Templates.Add("Shared\\Application.spark");
                }
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

    }
}