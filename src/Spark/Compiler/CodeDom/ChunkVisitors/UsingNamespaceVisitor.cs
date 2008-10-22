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
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Spark.Compiler.ChunkVisitors;

namespace Spark.Compiler.CodeDom.ChunkVisitors
{
    public class UsingNamespaceVisitor : ChunkVisitor
    {
        private readonly CodeCompileUnit _compileUnit;
        private readonly CodeNamespace _viewNamespace;

        private readonly Dictionary<string, object> _namespaceAdded = new Dictionary<string, object>();
        private readonly Dictionary<string, Assembly> _assemblyAdded = new Dictionary<string, Assembly>();

        readonly Stack<string> _noncyclic = new Stack<string>();


        public UsingNamespaceVisitor(CodeCompileUnit compileUnit, CodeNamespace viewNamespace)
        {
            _compileUnit = compileUnit;
            _viewNamespace = viewNamespace;
        }

        public ICollection<Assembly> Assemblies { get { return _assemblyAdded.Values; } }

        protected override void Visit(UseNamespaceChunk chunk)
        {
            UsingNamespace(chunk.Namespace);
        }
        protected override void Visit(UseAssemblyChunk chunk)
        {
            UsingAssembly(chunk.Assembly);
        }


        protected override void Visit(ExtensionChunk chunk)
        {
            //TODO: extensions are not CodeDom compatible
            //chunk.Extension.VisitChunk(this, OutputLocation.UsingNamespace, chunk.Body, _viewNamespace);
        }


        public void UsingNamespace(string ns)
        {
            if (_namespaceAdded.ContainsKey(ns))
                return;

            _namespaceAdded.Add(ns, null);
            _viewNamespace.Imports.Add(new CodeNamespaceImport(ns));
        }

        public void UsingAssembly(string assemblyString)
        {
            if (_assemblyAdded.ContainsKey(assemblyString))
                return;

            var assembly = Assembly.Load(assemblyString);
            _assemblyAdded.Add(assemblyString, assembly);

            _compileUnit.ReferencedAssemblies.Add(assembly.FullName);
        }
    }
}