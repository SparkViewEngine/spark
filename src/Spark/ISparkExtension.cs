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
using System.Collections.Generic;
using System.Text;
using Spark.Compiler;
using Spark.Compiler.ChunkVisitors;
using Spark.Compiler.NodeVisitors;
using Spark.Parser.Markup;

namespace Spark
{
    public interface ISparkExtensionFactory
    {
        ISparkExtension CreateExtension(VisitorContext context, ElementNode node);
    }

    public interface ISparkExtension
    {
        void VisitNode(INodeVisitor visitor, IList<Node> body, IList<Chunk> chunks);
        void VisitChunk(IChunkVisitor visitor, OutputLocation location, IList<Chunk> body, StringBuilder output);
    }

    public enum OutputLocation
    {
        NotWriting,
        UsingNamespace,
        ClassMembers,
        RenderMethod,
    }
}
