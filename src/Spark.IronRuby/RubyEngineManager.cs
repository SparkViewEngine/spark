using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronRuby;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Spark.IronRuby;
using Spark.IronRuby.Compiler.Ruby;

namespace Spark.IronRuby
{
    public class RubyEngineManager
    {
        private readonly ScriptEngine _scriptEngine;
        private readonly Dictionary<Guid, CompiledCode> _compiledViewScripts = new Dictionary<Guid, CompiledCode>();

        public RubyEngineManager()
        {
            _scriptEngine = Ruby.CreateEngine();
        }

        public ScriptEngine ScriptEngine
        {
            get { return _scriptEngine; }
        }

        public void InstanceCreated(RubyViewCompiler compiler, IScriptingSparkView view)
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

        public void InstanceReleased(RubyViewCompiler compiler, IScriptingSparkView view)
        {
        }
    }
}