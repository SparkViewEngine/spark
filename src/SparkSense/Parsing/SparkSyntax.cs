using System;
using Spark.Parser.Markup;
using Spark.Parser;
using System.Collections.Generic;
using Spark.Compiler.NodeVisitors;
using System.Collections;

namespace SparkSense.Parsing
{
    public class SparkSyntax
    {
        public static IList<Node> ParseNodes(string content)
        {
            var grammar = new MarkupGrammar();
            var result = grammar.Nodes(Source(content));
            return result.Value;
        }

        public static Node ParseNode(string content, int position)
        {
            int start, end;
            GetElementStartAndEnd(content, position, out start, out end);
            if (IsPositionOutsideANode(position, start, end) || IsPositionInClosingElement(content, start))
                return null;

            string openingElement = GetOpeningElement(content, position, start);
            var nodes = ParseNodes(openingElement);

            if (ElementNodeHasInvalidAttributes(nodes))
                ReconstructValidElementNode(ref nodes);

            return (nodes.Count > 0 ? nodes[0] : null);
        }

        private static void ReconstructValidElementNode(ref IList<Node> nodes)
        {
            TextNode elementStart = (TextNode)nodes[0];
            TextNode elementBody = (TextNode)nodes[1];
            if (!char.IsLetter(elementBody.Text.ToCharArray()[0])) return;

            var firstSpaceAfterStart = elementBody.Text.IndexOf(' ');
            var elementWithoutAttributes = String.Format("{0}{1}/>", elementStart.Text, elementBody.Text.Substring(0, firstSpaceAfterStart));
            nodes = ParseNodes(elementWithoutAttributes);
        }

        private static bool ElementNodeHasInvalidAttributes(IList<Node> nodes)
        {
            return nodes.Count == 2 && ((TextNode)nodes[0]).Text == "<";
        }

        public static Type ParseContext(string content, int position)
        {
            var previousChar = content.ToCharArray()[position - 1];
            switch (previousChar)
            {
                case '<':
                    return typeof(ElementNode);
                case ' ':
                    return typeof(AttributeNode);
                default:
                    break;
            }
            return typeof(TextNode);
        }

        public static bool IsSparkNode(Node currentNode, out Node sparkNode)
        {
            var visitor = new SpecialNodeVisitor(new VisitorContext());
            visitor.Accept(currentNode);
            sparkNode = visitor.Nodes.Count > 0 ? visitor.Nodes[0] : null;
            return sparkNode != null && sparkNode is SpecialNode;
        }

        private static void GetElementStartAndEnd(string content, int position, out int start, out int end)
        {
            start = content.LastIndexOf('<', position > 0 ? position - 1 : 0);
            end = content.LastIndexOf('>', position > 0 ? position - 1 : 0);
        }

        private static string GetOpeningElement(string content, int position, int start)
        {
            var nextStart = content.IndexOf('<', position);

            var fullElement = nextStart != -1
                ? content.Substring(start, nextStart - start)
                : content.Substring(start);
            if (!fullElement.Contains(">")) fullElement += "/>";
            else if (!fullElement.Contains("/>")) fullElement = fullElement.Replace(">", "/>");
            return fullElement;
        }

        private static bool IsPositionOutsideANode(int position, int start, int end)
        {
            return (end > start && end < position) || position == 0;
        }

        private static bool IsPositionInClosingElement(string content, int start)
        {
            return content.ToCharArray()[start + 1] == '/';
        }

        private static Position Source(string content)
        {
            return new Position(new SourceContext(content));
        }

        private IList<INodeVisitor> BuildNodeVisitors(VisitorContext context)
        {
            return new INodeVisitor[]
                       {
                           //new NamespaceVisitor(context),
                           //new IncludeVisitor(context),
                           //new PrefixExpandingVisitor(context),
                           new SpecialNodeVisitor(context),
                           //new CacheAttributeVisitor(context),
                           //new ForEachAttributeVisitor(context),
                           //new ConditionalAttributeVisitor(context),
                           //new OmitExtraLinesVisitor(context),
                           //new TestElseElementVisitor(context),
                           //new OnceAttributeVisitor(context),
                           //new UrlAttributeVisitor(context),
                           //new BindingExpansionVisitor(context)
                       };
        }

    }
}
