using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Spark;
using Spark.FileSystem;
using Spark.Web.Mvc;

namespace SkinsFolder
{
    /// <summary>
    /// Note! The workaround in here is because of a design flaw in the Spark compiled view
    /// holder. It should not be a singleton and must be refactored.
    /// </summary>
    public class ThemedViewFactory : IViewEngine
    {
        private readonly IViewFolder _defaultViewFolder;
        private readonly IViewEngine _defaultEngine;
        private readonly Dictionary<string, IViewEngine> _themedEngines = new Dictionary<string, IViewEngine>();

        public ThemedViewFactory()
        {
            var container = SparkEngineStarter.CreateContainer();
            _defaultViewFolder = container.GetService<IViewFolder>();
            _defaultEngine = container.GetService<IViewEngine>();
        }

        public void AddTheme(string name)
        {
            IViewFolder themeFolder = new VirtualPathProviderViewFolder("~/Themes/" + name);
            themeFolder = themeFolder.Append(_defaultViewFolder);

            var container = SparkEngineStarter.CreateContainer();
            container.SetService(themeFolder);
            _themedEngines.Add(name, container.GetService<IViewEngine>());
        }

        IViewEngine Associate(RequestContext context)
        {
            var themeCookie = context.HttpContext.Request.Cookies["theme"];
            if (themeCookie != null)
            {
                var name = themeCookie.Value;                
                IViewEngine engine;
                if (_themedEngines.TryGetValue(name, out engine))
                {
                    return engine;
                }
            }

            return _defaultEngine;
        }

        ViewEngineResult IViewEngine.FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            var viewEngine = Associate(controllerContext.RequestContext);
            return viewEngine.FindPartialView(controllerContext, partialViewName, useCache);
        }

        ViewEngineResult IViewEngine.FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            var viewEngine = Associate(controllerContext.RequestContext);
            return viewEngine.FindView(controllerContext, viewName, masterName, useCache);
        }

        void IViewEngine.ReleaseView(ControllerContext controllerContext, IView view)
        {
            var viewEngine = Associate(controllerContext.RequestContext);
            viewEngine.ReleaseView(controllerContext, view);
        }
    }
}
