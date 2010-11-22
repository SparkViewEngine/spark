using System.IO;
using System.Web.Mvc;
using Spark;
using Spark.FileSystem;

namespace ConsoleTemplating.Services
{
    public class DefaultMessageBuilder : MessageBuilder
    {
        private readonly ISparkViewEngine _engine;

        public DefaultMessageBuilder()
        {
			SparkSettings settings = new SparkSettings();
			settings.SetPageBaseType(typeof(TemplateBase));
            _engine = new SparkViewEngine(settings);
        }

        public override void Transform(string templateName, object data, TextWriter output)
        {
            var descriptor = new SparkViewDescriptor().AddTemplate(templateName + ".spark");

            var view = (TemplateBase)_engine.CreateInstance(descriptor);
            try
            {
                view.ViewData = new ViewDataDictionary(data);
                view.RenderView(output);
            }
            finally
            {
                _engine.ReleaseInstance(view);
            }
        }
    }
}
