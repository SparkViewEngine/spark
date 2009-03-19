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
using System;
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Spark.Ruby.Compiler;

namespace Spark.Ruby
{
    public class RubyEngineManager
    {
        private readonly ScriptEngine _scriptEngine;
        private readonly Dictionary<Guid, CompiledCode> _compiledViewScripts = new Dictionary<Guid, CompiledCode>();

        public RubyEngineManager()
        {
            _scriptEngine = IronRuby.Ruby.CreateEngine();
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