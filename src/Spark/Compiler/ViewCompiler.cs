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
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Spark.Compiler
{
    public abstract class ViewCompiler
    {
        protected ViewCompiler()
        {
            GeneratedViewId = Guid.NewGuid();
        }

        public SparkViewDescriptor Descriptor { get; set; }
        public string ViewClassFullName { get; set; }

        public string SourceCode { get; set; }
        public IList<SourceMapping> SourceMappings { get; set; }
        public Type CompiledType { get; set; }
        public Guid GeneratedViewId { get; set; }

        public string TargetNamespace => Descriptor?.TargetNamespace;

        public abstract void CompileView(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources);
        public abstract void GenerateSourceCode(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources);

        private static Dictionary<Type, Func<ISparkView>> _ctors = new Dictionary<Type, Func<ISparkView>>();
        public ISparkView CreateInstance()
        {
            return FastActivator<ISparkView>.New(CompiledType);
        }
    }

    public static class FastActivator<TTargetClass> where TTargetClass : class
    {
        private static Dictionary<Type, Func<TTargetClass>> _ctors = new Dictionary<Type, Func<TTargetClass>>();

        public static TTargetClass New(Type type)
        {
            if (!_ctors.ContainsKey(type))
            {
                var exp = Expression.New(type);
                var d = Expression.Lambda<Func<TTargetClass>>(exp).Compile();
                _ctors.Add(type, d);
            }

            return _ctors[type]();
        }
    }
}
