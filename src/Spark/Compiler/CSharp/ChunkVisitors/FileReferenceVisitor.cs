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
using Spark.Compiler.ChunkVisitors;

namespace Spark.Compiler.CSharp.ChunkVisitors
{
    public class FileReferenceVisitor : ChunkVisitor
    {
        private readonly IList<RenderPartialChunk> _references = new List<RenderPartialChunk>();

        //refactor: see how this is used and change it's type
        public IList<RenderPartialChunk> References
        {
            get { return _references; }
        }

        protected override void Visit(RenderPartialChunk chunk)
        {
            References.Add(chunk);
            Accept(chunk.Body);
            foreach (var chunks in chunk.Sections.Values)
                Accept(chunks);
        }

        protected override void Visit(UseImportChunk chunk)
        {
            References.Add(new RenderPartialChunk { Name = chunk.Name });
        }
    }
}