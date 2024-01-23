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
using Spark.Compiler;
using Spark.Compiler.CSharp;
using Spark.Compiler.Javascript;
using Spark.Compiler.VisualBasic;

namespace Spark
{
    public class DefaultLanguageFactory(IBatchCompiler batchCompiler) : ISparkLanguageFactory
    {
        public virtual ViewCompiler CreateViewCompiler(ISparkViewEngine engine, SparkViewDescriptor descriptor)
        {
            var pageBaseType = engine.Settings.PageBaseType;
            if (string.IsNullOrEmpty(pageBaseType))
            {
                pageBaseType = engine.DefaultPageBaseType;
            }

            var language = descriptor.Language;
            if (language == LanguageType.Default)
            {
                language = engine.Settings.DefaultLanguage;
            }

            ViewCompiler viewCompiler;
            switch (language)
            {
                case LanguageType.Default:
                case LanguageType.CSharp:
                    viewCompiler = new CSharpViewCompiler(batchCompiler);
                    break;
                case LanguageType.VisualBasic:
                    viewCompiler = new VisualBasicViewCompiler(batchCompiler);
                    break;
                case LanguageType.Javascript:
                    viewCompiler = new JavascriptViewCompiler();
                    break;
                default:
                    throw new CompilerException($"Unknown language type {descriptor.Language}");
            }

            viewCompiler.BaseClass = pageBaseType;
            viewCompiler.Descriptor = descriptor;
            viewCompiler.Debug = engine.Settings.Debug;
            viewCompiler.NullBehaviour = engine.Settings.NullBehaviour;
            viewCompiler.UseAssemblies = engine.Settings.UseAssemblies;
            viewCompiler.UseNamespaces = engine.Settings.UseNamespaces;
            
            return viewCompiler;
        }

        public virtual void InstanceCreated(ViewCompiler compiler, ISparkView view)
        {
        }

        public virtual void InstanceReleased(ViewCompiler compiler, ISparkView view)
        {
        }
    }
}
