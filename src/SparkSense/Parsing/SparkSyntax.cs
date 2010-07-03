using System;
using Spark.Parser.Markup;
using Spark.Parser;
using System.Collections.Generic;
using Spark.Compiler.NodeVisitors;

namespace SparkSense.Parsing
{
    public class SparkSyntax
    {
        public SparkSyntax()
        {

        }

        public IList<Node> ParseNodes(string content)
        {
            var grammar = new MarkupGrammar();
            var result = grammar.Nodes(Source(content));
            return result.Value;
        }

        public Node ParseNode(string content, int position)
        {
            var start = content.LastIndexOf('<', position - 1);
            var nextStart = content.IndexOf('<', position);

            var fullElement = nextStart != -1
                ? content.Substring(start, nextStart - start)
                : content.Substring(start);
            if (!fullElement.Contains(">")) fullElement += ">";

            var nodes = ParseNodes(fullElement);

            if (nodes.Count > 1 && nodes[0] is TextNode)
            {
                var firstSpaceAfterStart = content.IndexOf(' ', start) - start;
                var elementWithoutAttributes = content.Substring(start, firstSpaceAfterStart) + ">";
                nodes = ParseNodes(elementWithoutAttributes);
            }

            return ((ElementNode)nodes[0]);
        }

        public bool IsSparkNode(Node inputNode, out Node sparkNode)
        {
            var visitor = new SpecialNodeVisitor(new VisitorContext());
            visitor.Accept(inputNode);
            sparkNode = visitor.Nodes != null ? visitor.Nodes[0] : null;
            return sparkNode != null && sparkNode is SpecialNode;
        }

        //public SparkSyntaxTypes GetSyntaxType(char key)
        //{
        //    switch (key)
        //    {
        //        case '<':
        //            return SparkSyntaxTypes.Element;
        //        case ' ':
        //            return CheckForAttribute();
        //        case '{': //TODO: Check for preceeding $
        //            return SparkSyntaxTypes.Variable;
        //        case '"': //TODO Check for preceeding =
        //            return SparkSyntaxTypes.AttributeValue;
        //        default:
        //            if (Char.IsLetterOrDigit(key.ToString(), 0))
        //                return CheckWord();
        //            return SparkSyntaxTypes.None;
        //    }
        //}

        //private SparkSyntaxTypes CheckWord()
        //{
        //    if (_textExplorer.IsCurrentWordAnElement())
        //        return SparkSyntaxTypes.Element;
        //    return SparkSyntaxTypes.None;
        //}

        //private SparkSyntaxTypes CheckForAttribute()
        //{
        //    if (_textExplorer.IsPositionedInsideAnElement(_textExplorer.GetStartPosition())) return SparkSyntaxTypes.None;

        //    var node = _textExplorer.GetNodeAtPosition(_textExplorer.GetStartPosition());
        //    return node is ElementNode ? SparkSyntaxTypes.Attribute : SparkSyntaxTypes.None;
        //}

        private static Position Source(string content)
        {
            return new Position(new SourceContext(content));
        }

        private IList<INodeVisitor> BuildNodeVisitors(VisitorContext context)
        {
            return new INodeVisitor[]
                       {
                           new NamespaceVisitor(context),
                           new IncludeVisitor(context),
                           new PrefixExpandingVisitor(context),
                           new SpecialNodeVisitor(context),
                           new CacheAttributeVisitor(context),
                           new ForEachAttributeVisitor(context),
                           new ConditionalAttributeVisitor(context),
                           new OmitExtraLinesVisitor(context),
                           new TestElseElementVisitor(context),
                           new OnceAttributeVisitor(context),
                           new UrlAttributeVisitor(context),
                           //new BindingExpansionVisitor(context)
                       };
        }

    }
}
