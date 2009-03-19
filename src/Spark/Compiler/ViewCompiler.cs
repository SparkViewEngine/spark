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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Spark.Compiler.ChunkVisitors;
using Spark;

namespace Spark.Compiler
{
    public abstract class ViewCompiler
    {
        protected ViewCompiler()
        {
            GeneratedViewId = Guid.NewGuid();
        }

        public string BaseClass { get; set; }
        public SparkViewDescriptor Descriptor { get; set; }
        public string ViewClassFullName { get; set; }

        public string SourceCode { get; set; }
        public IList<SourceMapping> SourceMappings { get; set; }
        public Type CompiledType { get; set; }
        public Guid GeneratedViewId { get; set; }

        public bool Debug { get; set; }
		public NullBehaviour NullBehaviour { get; set; }
    	public IEnumerable<string> UseNamespaces { get; set; }
        public IEnumerable<string> UseAssemblies { get; set; }

        public string TargetNamespace
        {
            get
            {
                return Descriptor == null ? null : Descriptor.TargetNamespace;
            }
        }

        public abstract void CompileView(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources);
        public abstract void GenerateSourceCode(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources);

        public ISparkView CreateInstance()
        {
            return (ISparkView)Activator.CreateInstance(CompiledType);
        }

    }
}
