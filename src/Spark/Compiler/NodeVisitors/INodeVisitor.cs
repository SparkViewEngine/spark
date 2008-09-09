using System.Collections.Generic;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public interface INodeVisitor
    {
        VisitorContext Context { get; set; }
        IList<Node> Nodes { get; }

        void Accept(IList<Node> nodes);
        void Accept(Node node);
    }
}