using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.FileSystem;
using Spark.Parser.Markup;

namespace Spark.Parser.Syntax
{
    public class CSharpGrammar : CharGrammar
    {        
        public CSharpGrammar()
        {
            var expression = Ch("${").And(Rep1(ChNot('}'))).And(Ch('}'))
                .Build(hit => (Node)new ExpressionNode(hit.Left.Down));

            var statement = Opt(Ch('\r')).And(Ch('\n')).And(Rep(Ch(char.IsWhiteSpace))).And(Ch("//:")).And(Rep(ChNot('\r','\n')))
                .Build(hit => (Node)new StatementNode(hit.Down));

            var plaincode = Rep1(Ch(c => true).Unless(statement).Unless(expression)).Build(hit => (Node)new TextNode(hit));

            Nodes = Rep(statement.Or(expression).Or(plaincode));
        }

        public ParseAction<IList<Node>> Nodes;
    }
    public class CSharpSyntaxProvider : AbstractSyntaxProvider
    {
        static readonly CSharpGrammar _grammar = new CSharpGrammar();
        
        public override IList<Chunk> GetChunks(string viewPath, IViewFolder viewFolder, ISparkExtensionFactory extensionFactory)
        {
            var sourceContext = CreateSourceContext(viewPath, viewFolder);
            var position = new Position(sourceContext);

            var nodes = _grammar.Nodes(position);
            if (nodes.Rest.PotentialLength() != 0)
            {
                ThrowParseException(viewPath, position, nodes.Rest);
            }

            var chunkBuilder = new ChunkBuilderVisitor(nodes.Rest.GetPaint());
            chunkBuilder.Accept(nodes.Value);
            return chunkBuilder.Chunks;
        }
    }
}
