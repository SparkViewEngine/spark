/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Collections.Generic;
using System.Text;

namespace Spark.Compiler.ChunkVisitors
{
    public class UsingNamespaceVisitor : ChunkVisitor
    {
        private readonly StringBuilder _source;

        private readonly Dictionary<string, object> _used = new Dictionary<string, object>();

        readonly Stack<string> _noncyclic = new Stack<string>();


        public UsingNamespaceVisitor(StringBuilder output)
        {
            _source = output;
        }

        protected override void Visit(UseNamespaceChunk chunk)
        {
            Using(chunk.Namespace);
        }

        protected override void Visit(RenderPartialChunk chunk)
        {
            if (_noncyclic.Contains(chunk.FileContext.ViewSourcePath))
                return;

            _noncyclic.Push(chunk.FileContext.ViewSourcePath);
            Accept(chunk.FileContext.Contents);
            _noncyclic.Pop();
        }

        public void Using(string ns)
        {
            if (_used.ContainsKey(ns))
                return;

            _used.Add(ns, null);
            _source.AppendLine(string.Format("using {0};", ns));
        }
    }
}
