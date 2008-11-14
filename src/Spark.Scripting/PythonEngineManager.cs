using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Spark.Scripting.Compiler.Python;

namespace Spark.Scripting
{
    public class PythonEngineManager
    {
        private readonly ScriptEngine _scriptEngine;
        private readonly Dictionary<Guid, CompiledCode> _compiledViewScripts = new Dictionary<Guid, CompiledCode>();

        public PythonEngineManager()
        {
            _scriptEngine = Python.CreateEngine();
        }

        public ScriptEngine ScriptEngine
        {
            get { return _scriptEngine; }
        }

        public void InstanceCreated(PythonViewCompiler compiler, IScriptingSparkView view)
        {
            CompiledCode compiledCode;
            if (!_compiledViewScripts.TryGetValue(view.GeneratedViewId, out compiledCode))
            {
                var scriptSource = ScriptEngine.CreateScriptSourceFromString(view.ScriptSource, SourceCodeKind.File);
                compiledCode = scriptSource.Compile();
                _compiledViewScripts.Add(view.GeneratedViewId, compiledCode);
            }
            view.CompiledCode = compiledCode;
        }

        public void InstanceReleased(PythonViewCompiler compiler, IScriptingSparkView view)
        {
        }
    }
}
