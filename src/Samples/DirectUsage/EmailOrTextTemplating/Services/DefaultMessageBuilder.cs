using System.IO;
using System.Web.Mvc;
using EmailOrTextTemplating.Templates;
using Spark;
using Spark.FileSystem;

namespace EmailOrTextTemplating.Services
{
    public class DefaultMessageBuilder : MessageBuilder
    {
        private readonly ISparkViewEngine _engine;

        public DefaultMessageBuilder()
        {
            _engine = new SparkViewEngine();
        }

        public override void Transform(string templateName, object data, TextWriter output)
        {
            var descriptor = new SparkViewDescriptor()
                .AddTemplate(templateName + ".spark");

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
