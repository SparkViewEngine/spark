using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
using MvcContrib.ViewFactories;
using Spark;
using Spark.Compiler;
using Spark.FileSystem;

namespace MvcContrib.SparkViewEngine
{
    public class SparkViewFactory : IViewEngine, IViewFolder
    {
        public SparkViewFactory()
            : this(null)
        {
        }

        public SparkViewFactory(ISparkSettings settings)
        {
            Settings = settings ?? (ISparkSettings)ConfigurationManager.GetSection("spark") ?? new SparkSettings();
        }

        public ISparkSettings Settings { get; set; }

        private ISparkViewEngine _engine;
        public ISparkViewEngine Engine
        {
            get
            {
                if (_engine == null)
                    Engine = new Spark.SparkViewEngine(Settings);

                return _engine;
            }
            set
            {
                _engine = value;
                if (_engine != null)
                {
                    _engine.ViewFolder = this;
                    _engine.DefaultPageBaseType = typeof (SparkView).FullName;
                }
            }
        }

        private IViewSourceLoader _viewSourceLoader;
        public IViewSourceLoader ViewSourceLoader
        {
            get
            {
                if (_viewSourceLoader == null)
                    _viewSourceLoader = new FileSystemViewSourceLoader();

                return _viewSourceLoader;
            }
            set { _viewSourceLoader = value; }
        }

        public IViewActivatorFactory ViewActivatorFactory
        {
            get { return Engine.ViewActivatorFactory; }
            set { Engine.ViewActivatorFactory = value; }
        }

        [DebuggerNonUserCode]
        public void RenderView(ViewContext viewContext)
        {
            var descriptor = CreateDescriptor(viewContext);
            var entry = Engine.CreateEntry(descriptor);
            var view = (SparkView)entry.CreateInstance();
            view.RenderView(viewContext);
            entry.ReleaseInstance(view);
        }

        [DebuggerNonUserCode]
        public SparkViewDescriptor CreateDescriptor(ViewContext viewContext)
        {
            var controllerName = viewContext.RouteData.GetRequiredString("controller");
            var viewName = viewContext.ViewName;
            var masterName = viewContext.MasterName;
            var targetNamespace = viewContext.Controller.GetType().Namespace;

            return CreateDescriptor(targetNamespace, controllerName, viewName, masterName);
        }

        private SparkViewDescriptor CreateDescriptor(string targetNamespace, string controllerName, string viewName, string masterName)
        {
            var descriptor = new SparkViewDescriptor
                                 {
                                     TargetNamespace = targetNamespace
                                 };

            if (ViewSourceLoader.HasView(controllerName + "\\" + viewName + ".spark"))
            {
                descriptor.Templates.Add(controllerName + "\\" + viewName + ".spark");
            }
            else if (ViewSourceLoader.HasView("Shared\\" + viewName + ".spark"))
            {
                descriptor.Templates.Add("Shared\\" + viewName + ".spark");
            }
            else
            {
                throw new CompilerException(string.Format("Unable to find templates {0}\\{1}.spark or Shared\\{1}.spark", controllerName, viewName));
            }

            if (!string.IsNullOrEmpty(masterName))
            {
                if (ViewSourceLoader.HasView("Layouts\\" + masterName + ".spark"))
                {
                    descriptor.Templates.Add("Layouts\\" + masterName + ".spark");
                }
                else if (ViewSourceLoader.HasView("Shared\\" + masterName + ".spark"))
                {
                    descriptor.Templates.Add("Shared\\" + masterName + ".spark");
                }
                else
                {
                    throw new CompilerException(string.Format("Unable to find templates Layouts\\{1}.spark or Shared\\{1}.spark", controllerName,
                                                              masterName));
                }
            }
            else
            {
                if (ViewSourceLoader.HasView("Layouts\\" + controllerName + ".spark"))
                {
                    descriptor.Templates.Add("Layouts\\" + controllerName + ".spark");
                }
                else if (ViewSourceLoader.HasView("Shared\\" + controllerName + ".spark"))
                {
                    descriptor.Templates.Add("Shared\\" + controllerName + ".spark");
                }
                else if (ViewSourceLoader.HasView("Layouts\\Application.spark"))
                {
                    descriptor.Templates.Add("Layouts\\Application.spark");
                }
                else if (ViewSourceLoader.HasView("Shared\\Application.spark"))
                {
                    descriptor.Templates.Add("Shared\\Application.spark");
                }
            }
            return descriptor;
        }

        IViewFile IViewFolder.GetViewSource(string path)
        {
            return new ViewFile(ViewSourceLoader.GetViewSource(path));
        }

        IList<string> IViewFolder.ListViews(string path)
        {
            return ViewSourceLoader.ListViews(path);
        }

        bool IViewFolder.HasView(string path)
        {
            return ViewSourceLoader.HasView(path);
        }

        internal class ViewFile : IViewFile
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

        public Assembly Precompile(SparkBatchDescriptor batch)
        {
            return Engine.BatchCompilation(CreateDescriptors(batch));
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
                    foreach (var fileName in ViewSourceLoader.ListViews(controllerName))
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
                if (entry.LayoutNames.Count == 0)
                {
                    descriptors.Add(CreateDescriptor(
                                            entry.ControllerType.Namespace,
                                            controllerName,
                                            viewName,
                                            null /*masterName*/));
                }
                else
                {
                    foreach (var masterName in entry.LayoutNames)
                    {
                        descriptors.Add(CreateDescriptor(
                                            entry.ControllerType.Namespace,
                                            controllerName,
                                            viewName,
                                            string.Join(" ", masterName.ToArray())));
                    }
                }
            }

            return descriptors;
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
