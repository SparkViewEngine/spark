using System.Collections.Generic;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public interface INodeVisitor
    {
        IList<Node> Nodes { get; }

        void Accept(IList<Node> nodes);
        void Accept(Node node);
    }
}