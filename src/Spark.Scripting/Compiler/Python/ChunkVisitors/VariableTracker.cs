using System.Collections.Generic;

namespace Spark.Scripting.Compiler.Python.ChunkVisitors
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