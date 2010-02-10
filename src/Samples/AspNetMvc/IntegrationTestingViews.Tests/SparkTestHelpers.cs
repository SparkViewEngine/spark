using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Spark;
using Spark.FileSystem;
using Spark.Web.Mvc;

namespace IntegrationTestingViews.Tests
{
    public class SparkTestHelpers
    {
        public static string RenderTemplate(string template)
        {
            return RenderTemplate(template, new ViewDataDictionary());
        }

        public static string RenderTemplate(string template, ViewDataDictionary viewData)
        {
            // Set up your spark engine goodness. 
            var settings = new SparkSettings().SetPageBaseType(typeof(SparkView));

            var engine = new SparkViewEngine(settings)
                         {
                             ViewFolder = new FileSystemViewFolder(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\IntegrationTestingViews\Views")
                         };

            // "Describe" the view (the template, it is a template after all), and its details.
            var descriptor = new SparkViewDescriptor().AddTemplate(template);

            // Create a spark view engine instance
            var view = (SparkView)engine.CreateInstance(descriptor);
            try
            {
                // Merge the view data. 
                viewData.Keys.ToList().ForEach(x => view.ViewData[x] = viewData[x]);

                // Render the view to a text writer. 
                var writer = new StringWriter();
                view.RenderView(writer);
                return writer.ToString();
            }
            finally
            {
                engine.ReleaseInstance(view);
            }
        }


        public static string RenderPartialView(string controllerName, string viewName)
        {
            return RenderViewImpl(controllerName, viewName, null, null,
                (engine, context) => engine.FindPartialView(context, viewName, false));
        }
        public static string RenderPartialView(string controllerName, string viewName, ViewDataDictionary viewData)
        {
            return RenderViewImpl(controllerName, viewName, viewData, null,
                (engine, context) => engine.FindPartialView(context, viewName, false));
        }
        public static string RenderPartialView(string controllerName, string viewName, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            return RenderViewImpl(controllerName, viewName, viewData, tempData,
                (engine, context) => engine.FindPartialView(context, viewName, false));
        }

        public static string RenderView(string controllerName, string viewName, string layoutName)
        {
            return RenderViewImpl(controllerName, viewName, null, null,
                (engine, context) => engine.FindView(context, viewName, layoutName, false));
        }
        public static string RenderView(string controllerName, string viewName, string layoutName, ViewDataDictionary viewData)
        {
            return RenderViewImpl(controllerName, viewName, viewData, null,
                (engine, context) => engine.FindView(context, viewName, layoutName, false));
        }
        public static string RenderView(string controllerName, string viewName, string layoutName, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            return RenderViewImpl(controllerName, viewName, viewData, tempData,
                (engine, context) => engine.FindView(context, viewName, layoutName, false));
        }

        public static string RenderViewImpl(string controllerName, string viewName, ViewDataDictionary viewData, TempDataDictionary tempData,
            Func<IViewEngine, ControllerContext, ViewEngineResult> findView)
        {
            var settings = new SparkSettings();
            var viewEngine = new SparkViewFactory(settings);
            viewEngine.ViewFolder =
                new FileSystemViewFolder(AppDomain.CurrentDomain.BaseDirectory +
                                         @"\..\..\..\IntegrationTestingViews\Views");

            var routeData = new RouteData();
            routeData.Values["controller"] = controllerName;

            var controllerContext = new ControllerContext(
                new HttpContextStub(),
                routeData,
                new ControllerStub());

            var view = (SparkView)findView(viewEngine, controllerContext).View;
            try
            {
                var writer = new StringWriter();
                var viewContext = new ViewContext(
                    controllerContext,
                    view,
                    viewData ?? new ViewDataDictionary(),
                    tempData ?? new TempDataDictionary(),
                    writer);

                view.Render(viewContext, writer);
                return writer.ToString();
            }
            finally
            {
                viewEngine.Engine.ReleaseInstance(view);
            }
        }


        public class ControllerStub : ControllerBase
        {
            protected override void ExecuteCore()
            {
                throw new NotImplementedException();
            }
        }

        public class HttpContextStub : HttpContextBase
        {
        }
    }
}