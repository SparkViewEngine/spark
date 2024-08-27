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
using System.IO;
using System.Linq;
using System.Text;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.FileSystem;
using Spark.Parser.Code;
using Spark.Parser.Markup;

namespace Spark.Parser.Syntax
{
    public class CSharpGrammar : CharGrammar
    {        
        public CSharpGrammar()
        {
            var expression = Ch("${").And(Rep1(ChNot('}'))).And(Ch('}'))
                .Build(hit => (Node)new ExpressionNode(new string(hit.Left.Down.ToArray())));

            var statement = Opt(Ch('\r')).And(Ch('\n')).And(Rep(Ch(char.IsWhiteSpace))).And(Ch("//:")).And(Rep(ChNot('\r','\n')))
                .Build(hit => (Node)new StatementNode(new string(hit.Down.ToArray())));

            var plaincode = Rep1(Ch(c => true).Unless(statement).Unless(expression)).Build(hit => (Node)new TextNode(hit));

            Nodes = Rep(statement.Or(expression).Or(plaincode));
        }

        public ParseAction<IList<Node>> Nodes;
    }

    public class CSharpSyntaxProvider : AbstractSyntaxProvider
    {
        static readonly CSharpGrammar _grammar = new CSharpGrammar();

        public override IList<Chunk> GetChunks(VisitorContext context, string path)
        {
            context.ViewPath = path;
            var sourceContext = CreateSourceContext(context.ViewPath, context.ViewFolder);
            var position = new Position(sourceContext);

            var nodes = _grammar.Nodes(position);
            if (nodes.Rest.PotentialLength() != 0)
            {
                ThrowParseException(context.ViewPath, position, nodes.Rest);
            }
            context.Paint = nodes.Rest.GetPaint();

            var chunkBuilder = new ChunkBuilderVisitor(context);
            chunkBuilder.Accept(nodes.Value);
            return chunkBuilder.Chunks;
        }

        public override IList<Node> IncludeFile(VisitorContext context, string path, string parse)
        {
            throw new System.NotImplementedException();
        }

        public override Snippets ParseFragment(Position begin, Position end)
        {
            throw new System.NotImplementedException();
        }
    }
}
