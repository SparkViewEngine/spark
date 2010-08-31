using Spark.Web.FubuMVC.Bootstrap;
using FubuMVC.Core;
using StructureMap;
using Spark.Web.FubuMVC;

namespace FubuMVC.MigrationSample
{
    public class Global : SparkStructureMapApplication
    {
        public override FubuRegistry GetMyRegistry()
        {
            var sparkViewFactory = ObjectFactory.Container.GetInstance<SparkViewFactory>();
            return new MigrationSampleRegistry(EnableDiagnostics, ControllerAssembly, sparkViewFactory);
        }
    }
}