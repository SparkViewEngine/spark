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
using System;
using Spark.Compiler.ChunkVisitors;
using Spark.Parser.Code;

namespace Spark.Compiler.CSharp.ChunkVisitors
{
    public class BaseClassVisitor : ChunkVisitor
    {
        public Snippets BaseClass { get; set; }
        public Snippets TModel { get; set; }

        bool _encounteredBaseClass;
        bool _encounteredTModel;

        public Snippets BaseClassTypeName
        {
            get
            {
                var baseClass = BaseClass;
                if (Snippets.IsNullOrEmpty(baseClass))
                    baseClass = "Spark.SparkViewBase";

                if (Snippets.IsNullOrEmpty(TModel))
                    return baseClass;

                var s = new Snippets();
                s.AddRange(baseClass);
                s.Add(new Snippet { Value = "<" });
                s.AddRange(TModel);
                s.Add(new Snippet { Value = ">" });
                return s;
            }
        }

        protected override void Visit(ViewDataModelChunk chunk)
        {
            if (_encounteredTModel && !string.Equals(TModel, chunk.TModel, StringComparison.Ordinal))
            {
                throw new CompilerException(string.Format("Only one viewdata model can be declared. {0} != {1}", TModel,
                                                          chunk.TModel));
            }
            TModel = chunk.TModel;
            _encounteredTModel = true;
        }

        protected override void Visit(PageBaseTypeChunk chunk)
        {
            if (_encounteredBaseClass && !string.Equals(BaseClass, chunk.BaseClass, StringComparison.Ordinal))
            {
                throw new CompilerException(string.Format("Only one pageBaseType can be declared. {0} != {1}", BaseClass,
                                                          chunk.BaseClass));
            }
            BaseClass = chunk.BaseClass;
            _encounteredBaseClass = true;
        }
    }
}
