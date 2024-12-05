// Copyright 2008-2024 Louis DeJardin
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
using System.Collections.Generic;

namespace Spark.Ruby.Compiler.ChunkVisitors
{
    public class VariableTracker
    {
        private Scope _scope;

        public VariableTracker(IDictionary<string, object> globalSymbols)
        {
            _scope = new Scope(new Scope(null) { Variables = globalSymbols });
        }

        public void PushScope()
        {
            _scope = new Scope(_scope);
        }

        public void PopScope()
        {
            _scope = _scope.Prior;
        }

        public void Declare(string name)
        {
            _scope.Variables.Add(name, null);
        }

        public bool IsDeclared(string name)
        {
            var scan = _scope;
            while (scan != null)
            {
                if (scan.Variables.ContainsKey(name))
                    return true;
                scan = scan.Prior;
            }
            return false;
        }

        class Scope
        {
            public Scope(Scope prior)
            {
                Variables = new Dictionary<string, object>();
                Prior = prior;
            }
            public IDictionary<string, object> Variables { get; set; }
            public Scope Prior { get; set; }
        }
    }
}