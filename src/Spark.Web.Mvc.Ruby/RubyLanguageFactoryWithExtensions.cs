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
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web.Mvc;
using System.Linq;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Spark.Compiler;
using Spark.Ruby;
using Spark.Ruby.Compiler;

[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.FormExtensions))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.InputExtensions))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.LinkExtensions))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.MvcForm))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.RenderPartialExtensions))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.SelectExtensions))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.TextAreaExtensions))]
[assembly: ExtensionType(typeof(HtmlHelper), typeof(System.Web.Mvc.Html.ValidationExtensions))]
[assembly: ExtensionType(typeof(AjaxHelper), typeof(System.Web.Mvc.Ajax.AjaxExtensions))]

namespace Spark.Web.Mvc.Ruby
{
    public class RubyLanguageFactoryWithExtensions : RubyLanguageFactory
    {
        public RubyLanguageFactoryWithExtensions(IBatchCompiler batchCompiler, ISparkSettings settings) : base(batchCompiler, settings)
        {
        }

        private bool _initialized;

        private string _scriptHeader;

        public override ViewCompiler CreateViewCompiler(ISparkViewEngine engine, SparkViewDescriptor descriptor)
        {
            Initialize();
            var viewCompiler = base.CreateViewCompiler(engine, descriptor);
            if (viewCompiler is RubyViewCompiler)
            {
                ((RubyViewCompiler)viewCompiler).ScriptHeader = _scriptHeader;
            }
            return viewCompiler;
        }

        private void Initialize()
        {
            if (_initialized)
                return;

            lock (this)
            {
                if (_initialized)
                    return;

                // need to load the assembly into the runtime domain
                // before any scripts are created in order for the extension
                // methods to be "seen" on the dynamic type.

                var languageFactoryAssembly = typeof(RubyLanguageFactoryWithExtensions).Assembly;
                _scriptHeader = BuildScriptHeader(languageFactoryAssembly);

                RubyEngineManager.ScriptEngine.Runtime.LoadAssembly(languageFactoryAssembly);
                RubyEngineManager.ScriptEngine.Runtime.LoadAssembly(typeof(HtmlHelper).Assembly);

                _initialized = true;
            }
        }

        public string BuildScriptHeader(Assembly languageFactoryAssembly)
        {
            // scan for extension types to monkey patch - ironruby doesn't have the
            // automagic wire-up implemented yet

            var extensionMethods = new Dictionary<MethodInfo, Type>();

            var assemblyExtensionAttrs = languageFactoryAssembly.GetCustomAttributes(typeof(ExtensionTypeAttribute), true);
            foreach (var assemblyExtension in assemblyExtensionAttrs.Cast<ExtensionTypeAttribute>())
            {
                foreach (var methodInfo in assemblyExtension.ExtensionType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod))
                {
                    var methodExtensionAttrs = methodInfo.GetCustomAttributes(typeof(ExtensionAttribute), true);
                    if (methodExtensionAttrs == null || methodExtensionAttrs.Length == 0)
                        continue;

                    extensionMethods.Add(methodInfo, assemblyExtension.ExtensionType);
                }
            }

            var builder = new StringBuilder();

            var htmlEntries = extensionMethods
                .Where(kv => kv.Key.GetParameters()[0].ParameterType == typeof(HtmlHelper))
                .Select(kv => ":" + kv.Key.Name + "=>" + RubyUtils.GetQualifiedName(kv.Value))
                .Distinct();

            var ajaxEntries = extensionMethods
                .Where(kv => kv.Key.GetParameters()[0].ParameterType == typeof(AjaxHelper))
                .Select(kv => ":" + kv.Key.Name + "=>" + RubyUtils.GetQualifiedName(kv.Value))
                .Distinct();

            builder.AppendLine("$htmlExtensionMethods = {");
            builder.AppendLine(string.Join(",\r\n", htmlEntries.ToArray()));
            builder.AppendLine("}");
            builder.AppendLine("$ajaxExtensionMethods = {");
            builder.AppendLine(string.Join(",\r\n", ajaxEntries.ToArray()));
            builder.AppendLine("}");

            builder.AppendLine(@"
view_html = view.html
def view_html.method_missing(name, *parameters)
 extensionObject = $htmlExtensionMethods[name]
 if (extensionObject != nil)
   extensionObject.send(name, self, *parameters)
 else
   System::Web::Mvc::HtmlHelper.send(name, *parameters)
 end
end
view_ajax = view.ajax
def view_ajax.method_missing(name, *parameters)
 extensionObject = $ajaxExtensionMethods[name]
 if (extensionObject != nil)
   extensionObject.send(name, self, *parameters)
 else
   System::Web::Mvc::AjaxHelper.send(name, *parameters)
 end
end
");
            return builder.ToString();
        }
    }
}