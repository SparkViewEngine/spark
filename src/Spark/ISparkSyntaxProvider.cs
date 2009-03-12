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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Code;
using Spark.Parser.Markup;

namespace Spark
{
    public interface ISparkSyntaxProvider
    {
        IList<Chunk> GetChunks(VisitorContext context, string path);
        IList<Node> IncludeFile(VisitorContext context, string path, string parse);
        Snippets ParseFragment(Position begin, Position end);
    }
}
