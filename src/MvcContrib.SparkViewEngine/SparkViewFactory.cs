using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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
            if (Settings.PageBaseType == null)
                Settings.PageBaseType = typeof(SparkView).FullName;
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
                    _engine.ViewFolder = this;
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
            get { return Engine.ViewActivatorFactory;}
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

            var descriptor = new SparkViewDescriptor
                                 {
                                     TargetNamespace = viewContext.Controller.GetType().Namespace
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
                    throw new CompilerException(string.Format("Unable to find templates {0}\\{1}.spark or Shared\\{1}.spark", controllerName,
                                                viewName));
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
    }
}
