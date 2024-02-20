// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
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
        public PythonLanguageFactoryWithExtensions(IBatchCompiler batchCompiler, ISparkSettings settings) : base(batchCompiler, settings)
        {
        }

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
                PythonEngineManager.ScriptEngine.Runtime.LoadAssembly(typeof (PythonLanguageFactoryWithExtensions).Assembly);
            }
        }
    }
}