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
using Spark.Compiler.ChunkVisitors;
using Spark.Parser.Code;

namespace Spark.Compiler.CSharp.ChunkVisitors
{
    public class BaseClassVisitor : ChunkVisitor
    {
        public string BaseClass { get; set; }
        public Snippets TModel { get; set; }

        public Snippets BaseClassTypeName
        {
            get
            {
                if (Snippets.IsNullOrEmpty(TModel))
                    return BaseClass ?? "Spark.SparkViewBase";

                var s = new Snippets();
                s.Add(new Snippet { Value = BaseClass ?? "Spark.SparkViewBase" });
                s.Add(new Snippet { Value = "<" });
                s.AddRange(TModel);
                s.Add(new Snippet { Value = ">" });
                return s;
            }
        }

        protected override void Visit(ViewDataModelChunk chunk)
        {
            if (!Snippets.IsNullOrEmpty(TModel) && TModel != chunk.TModel)
            {
                throw new CompilerException(string.Format("Only one viewdata model can be declared. {0} != {1}", TModel,
                                                          chunk.TModel));
            }
            TModel = chunk.TModel;
        }
    }
}