using System.Web.Mvc;
using Microsoft.Scripting.Runtime;
using Spark.Compiler;
using Spark.Python;

[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.FormExtensions))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.InputExtensions))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.LinkExtensions))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.MvcForm))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.RenderPartialExtensions))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.SelectExtensions))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.TextAreaExtensions))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.ValidationExtensions))]
[assembly: ExtensionType(typeof(AjaxHelper), typeof(System.Web.Mvc.Ajax.AjaxExtensions))]

namespace Spark.Web.Mvc.Python
{
    public class PythonLanguageFactoryWithExtensions : PythonLanguageFactory
    {
        private bool _initialized;

        public override ViewCompiler CreateViewCompiler(ISparkViewEngine engine, SparkViewDescriptor descriptor)
        {
            Initialize();
            return base.CreateViewCompiler(engine, descriptor);
        }

        private void Initialize()
        {
            if (_initialized)
                return;

            lock (this)
            {
                if (_initialized)
                    return;

                _initialized = true;

                // need to load the assembly into the runtime domain
                // before any scripts are created in order for the extension
                // methods to be "seen" on the dynamic type.
                PythonEngineManager.ScriptEngine.Runtime.LoadAssembly(
                    typeof (PythonLanguageFactoryWithExtensions).Assembly);
            }
        }
    }
}