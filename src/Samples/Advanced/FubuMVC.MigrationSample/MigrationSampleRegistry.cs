using Spark.Web.FubuMVC.Bootstrap;
using JavascriptViewResultSample.Controllers;
using Spark.Web.FubuMVC.ViewCreation;
using Spark.Web.FubuMVC.Extensions;
using FubuMVC.Core.Registration.Nodes;
using Spark.Web.FubuMVC;
using System;

namespace FubuMVC.MigrationSample
{
    public class MigrationSampleRegistry : SparkDefaultStructureMapRegistry
    {
        private SparkViewFactory _viewFactory;
        public MigrationSampleRegistry(bool debuggingEnabled, string controllerAssembly, SparkViewFactory viewFactory)
            : base(debuggingEnabled, controllerAssembly)
        {
            _viewFactory = viewFactory;

            //HomeIs<HomeController>(c => c.List(new ListInputModel() { Page = 1 }));

            Output.To(call => new JavaScriptOutputNode(GetJavaScriptViewToken(call), call))
                .WhenTheOutputModelIs<JavaScriptResponse>();
        }

        private SparkViewToken GetJavaScriptViewToken(ActionCall call)
        {
            var response = JavaScriptResponse.GetResponse(call);
            string viewName = response.ViewName;
            string controllerName = call.HandlerType.Name.RemoveSuffix("Controller");
            return _viewFactory.GetViewToken(call, controllerName, viewName, Spark.LanguageType.Javascript);
        }

    }
}
