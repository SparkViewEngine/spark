using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Mvc;
using MvcContrib.ViewFactories;
using Spark;
using Spark.Compiler;

namespace MvcContrib.SparkViewEngine
{
    public class SparkViewFactory : IViewEngine
    {
        public SparkViewFactory()
            : this(new FileSystemViewSourceLoader())
        {

        }

        public SparkViewFactory(IViewSourceLoader viewSourceLoader)
        {
            _viewSourceLoaderWrapper = new ViewSourceLoaderWrapper(viewSourceLoader);

            Engine = new Spark.SparkViewEngine(
                "MvcContrib.SparkViewEngine.SparkView",
                _viewSourceLoaderWrapper);
        }

        private readonly ViewSourceLoaderWrapper _viewSourceLoaderWrapper;

        public ISparkViewEngine Engine { get; set; }

        public IViewSourceLoader ViewSourceLoader
        {
            get { return _viewSourceLoaderWrapper.ViewSourceLoader; }
            set { _viewSourceLoaderWrapper.ViewSourceLoader = value; }
        }

        [DebuggerNonUserCode]
        public void RenderView(ViewContext viewContext)
        {
            SparkViewDescriptor descriptor = CreateDescriptor(viewContext);
            var view = (SparkView)Engine.CreateInstance(descriptor);
            view.RenderView(viewContext);
        }

        [DebuggerNonUserCode]
        public SparkViewDescriptor CreateDescriptor(ViewContext viewContext)
        {
            var controllerName = viewContext.RouteData.GetRequiredString("controller");
            var viewName = viewContext.ViewName;
            var masterName = viewContext.MasterName;

            var descriptor = new SparkViewDescriptor();
            if (_viewSourceLoaderWrapper.HasView(controllerName + "\\" + viewName + ".spark"))
            {
                descriptor.Templates.Add(controllerName + "\\" + viewName + ".spark");
            }
            else if (_viewSourceLoaderWrapper.HasView("Shared\\" + viewName + ".spark"))
            {
                descriptor.Templates.Add("Shared\\" + viewName + ".spark");
            }
            else
            {
                throw new CompilerException(string.Format("Unable to find templates {0}\\{1}.spark or Shared\\{1}.spark", controllerName, viewName));
            }

            if (!string.IsNullOrEmpty(masterName))
            {
                if (_viewSourceLoaderWrapper.HasView("Layouts\\" + masterName + ".spark"))
                {
                    descriptor.Templates.Add("Layouts\\" + masterName + ".spark");
                }
                else if (_viewSourceLoaderWrapper.HasView("Shared\\" + masterName + ".spark"))
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
                if (_viewSourceLoaderWrapper.HasView("Layouts\\" + controllerName + ".spark"))
                {
                    descriptor.Templates.Add("Layouts\\" + controllerName + ".spark");
                }
                else if (_viewSourceLoaderWrapper.HasView("Shared\\" + controllerName + ".spark"))
                {
                    descriptor.Templates.Add("Shared\\" + controllerName + ".spark");
                }
                else if (_viewSourceLoaderWrapper.HasView("Layouts\\Application.spark"))
                {
                    descriptor.Templates.Add("Layouts\\Application.spark");
                }
                else if (_viewSourceLoaderWrapper.HasView("Shared\\Application.spark"))
                {
                    descriptor.Templates.Add("Shared\\Application.spark");
                }
            }
            return descriptor;
        }        
    }
}
