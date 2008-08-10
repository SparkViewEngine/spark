using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler.NodeVisitors;
using Spark.Parser;
using Spark.Parser.Markup;

namespace Spark.Tests
{
    public class BaseVisitorTester
    {
        MarkupGrammar _grammar = new MarkupGrammar();

        public IList<Node> ParseNodes(string content)
        {
            return _grammar.Nodes(new Position(new SourceContext(content))).Value;
        }


        public IList<Node> ParseNodes(string content, params AbstractNodeVisitor[] visitors)
        {
            var nodes = _grammar.Nodes(new Position(new SourceContext(content))).Value;
            foreach(var visitor in visitors)
            {
                visitor.Accept(nodes);
                nodes = visitor.Nodes;
            }
            return nodes;
        }
    }
}
