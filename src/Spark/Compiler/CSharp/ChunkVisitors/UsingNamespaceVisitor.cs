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
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Spark.Compiler.ChunkVisitors;
using Spark.Parser.Code;

namespace Spark.Compiler.CSharp.ChunkVisitors
{
    public class UsingNamespaceVisitor : ChunkVisitor
    {
        private readonly SourceWriter _source;

        private readonly Dictionary<string, object> _namespaceAdded = new Dictionary<string, object>();
        private readonly Dictionary<string, Assembly> _assemblyAdded = new Dictionary<string, Assembly>();

        readonly Stack<string> _noncyclic = new Stack<string>();


        public UsingNamespaceVisitor(SourceWriter output)
        {
            _source = output;
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
            chunk.Extension.VisitChunk(this, OutputLocation.UsingNamespace, chunk.Body, _source.GetStringBuilder());
        }

        protected override void Visit(RenderPartialChunk chunk)
        {
            if (_noncyclic.Contains(chunk.FileContext.ViewSourcePath))
                return;

            _noncyclic.Push(chunk.FileContext.ViewSourcePath);
            Accept(chunk.FileContext.Contents);
            _noncyclic.Pop();
        }

        public void UsingNamespace(Snippets ns)
        {
            if (_namespaceAdded.ContainsKey(ns))
                return;

            _namespaceAdded.Add(ns, null);
            _source.Write("using ").WriteCode(ns).WriteLine(";");
        }

        public void UsingAssembly(string assemblyString)
        {
            if (_assemblyAdded.ContainsKey(assemblyString))
                return;

            var assembly = Assembly.Load(assemblyString);
            _assemblyAdded.Add(assemblyString, assembly);
        }
    }
}