// Copyright 2008 Louis DeJardin - http://whereslou.com
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
using System.Threading;
using Spark.Compiler;
using Spark.Python.Compiler;

namespace Spark.Python
{
    public class ScriptingLanguageFactory : DefaultLanguageFactory
    {
        private PythonEngineManager _PythonEngineManager;
        public PythonEngineManager PythonEngineManager
        {
            get
            {
                if (_PythonEngineManager == null)
                    Interlocked.CompareExchange(ref _PythonEngineManager, new PythonEngineManager(), null);
                return _PythonEngineManager;
            }
        }

        public override ViewCompiler CreateViewCompiler(ISparkViewEngine engine, SparkViewDescriptor descriptor)
        {
            ViewCompiler viewCompiler;
            switch (descriptor.Language)
            {
                case LanguageType.Default:
                case LanguageType.Python:
                    viewCompiler = new PythonViewCompiler();
                    break;
                default:
                    return base.CreateViewCompiler(engine, descriptor);
            }

            var pageBaseType = engine.Settings.PageBaseType;
            if (string.IsNullOrEmpty(pageBaseType))
                pageBaseType = engine.DefaultPageBaseType;

            viewCompiler.BaseClass = pageBaseType;
            viewCompiler.Descriptor = descriptor;
            viewCompiler.Debug = engine.Settings.Debug;
        	viewCompiler.NullBehaviour = engine.Settings.NullBehaviour;
            viewCompiler.UseAssemblies = engine.Settings.UseAssemblies;
            viewCompiler.UseNamespaces = engine.Settings.UseNamespaces;
            return viewCompiler;
        }

        public override void InstanceCreated(ViewCompiler compiler, ISparkView view)
        {
            if (compiler is PythonViewCompiler && view is IScriptingSparkView)
            {
                PythonEngineManager.InstanceCreated(
                    (PythonViewCompiler) compiler,
                    (IScriptingSparkView) view);
            }
            else
            {
                base.InstanceCreated(compiler, view);
            }
        }

        public override void InstanceReleased(ViewCompiler compiler, ISparkView view)
        {
            if (compiler is PythonViewCompiler && view is IScriptingSparkView)
            {
                PythonEngineManager.InstanceReleased(
                    (PythonViewCompiler)compiler,                                                    
                    (IScriptingSparkView)view);
            }
            else
            {
                base.InstanceCreated(compiler, view);
            }
        }
    }
}
