using System;
using System.Linq;
using Spark.Parser.Markup;
using Spark.Parser;
using System.Collections.Generic;
using Spark.Compiler.NodeVisitors;
using System.Collections;
using Spark.Compiler;

namespace SparkSense.Parsing
{
    public class SparkSyntax
    {
        private const char COLON = ':';
        private const char OPEN_ELEMENT = '<';
        private const char SPACE = ' ';
        private const char DOUBLE_QUOTE = '"';
        private const char SINGLE_QUOTE = '\'';
        private const char OPEN_BRACE = '{';
        private const char CLOSE_ELEMENT = '>';
        private const char CLOSE_BRACE = '}';
        private const string SELF_CLOSE_ELEMENT = "/>";
        private const char FWD_SLASH = '/';
        private const char EXCLAMATION = '!';
        private const char DOLLAR = '$';

        public static IList<Node> ParseNodes(string content)
        {
            var grammar = new MarkupGrammar();
            var result = grammar.Nodes(Source(content));
            return result.Value;
        }

        public static Node ParseNode(string content, int position)
        {
            int start, end;
            GetFragmentStartAndEnd(content, position, out start, out end);
            if (IsPositionOutsideANode(position, start, end) || IsPositionInClosingElement(content, start))
                return null;

            string openingFragment = GetOpeningFragment(content, position, start);
            var nodes = ParseNodes(openingFragment);

            if (ElementNodeHasInvalidAttributes(nodes))
                ReconstructValidElementNode(ref nodes);

            return (nodes.Count > 0 ? nodes[0] : null);
        }

        private static bool ElementNodeHasInvalidAttributes(IList<Node> nodes)
        {
            return nodes.Count == 2 && nodes[0] is TextNode && ((TextNode)nodes[0]).Text == OPEN_ELEMENT.ToString();
        }

        public static Type ParseContext(string content, int position)
        {
            var contentChars = content.ToCharArray();
            var previousChar = contentChars[position - 1];
            switch (previousChar)
            {
                case OPEN_ELEMENT:
                case COLON:
                    return typeof(ElementNode);
                case SPACE:
                case DOUBLE_QUOTE:
                case SINGLE_QUOTE:
                    return typeof(AttributeNode);
                case OPEN_BRACE:
                    if (IsExpression(content, position))
                        return typeof(ExpressionNode);
                    break;
                default:
                    break;
            }

            if (IsPositionInElementName(content, position))
                return typeof(ElementNode);

            return typeof(TextNode);
        }
        
        public static Type ParseContextChunk(string content, int position)
        {
            var node = ParseNode(content, position);
            var contentToParse = node is ElementNode ? GetElementNodeAsString((ElementNode)node) : content;

            var grammar = new MarkupGrammar();
            var visitorContext = new VisitorContext();
            var result = grammar.Nodes(Source(contentToParse));
            var nodes = result.Value;
            foreach (var visitor in BuildChunkVisitors(visitorContext))
            {
                visitor.Accept(nodes);
                nodes = visitor.Nodes;
            }

            var chunkBuilder = new ChunkBuilderVisitor(visitorContext);
            chunkBuilder.Accept(nodes);
            var chunks = chunkBuilder.Chunks;

            if (chunks.Count == 1)
            {
                var chunkTypeToReturn = chunks[0].GetType();
                if (chunkTypeToReturn == typeof(ScopeChunk))
                {
                    chunkTypeToReturn = ((ScopeChunk)chunks[0]).Body[0].GetType();
                }
                return chunkTypeToReturn;
            }

            return typeof(SendLiteralChunk);
        }

        public static bool IsSparkNode(Node currentNode, out Node sparkNode)
        {
            IList<Node> resultNodes = null;
            if (currentNode != null)
                foreach (var visitor in BuildNodeVisitors(new VisitorContext()))
                {
                    visitor.Accept(currentNode);
                    resultNodes = visitor.Nodes;
                }
            sparkNode = resultNodes != null && resultNodes.Count > 0 ? resultNodes[0] : null;
            return sparkNode != null && sparkNode is SpecialNode;
        }

        private static bool IsElement(string content, int position)
        {
            int start, end;
            GetFragmentStartAndEnd(content, position, out start, out end);
            return
                start > -1 &&
                !IsPositionOutsideANode(position, start, end) &&
                content.ToCharArray()[start] == OPEN_ELEMENT;
        }

        private static bool IsExpression(string content, int position)
        {
            int start, end;
            GetFragmentStartAndEnd(content, position, out start, out end);

            var contentChars = content.ToCharArray();
            return start > -1 &&
                (contentChars[start] == DOLLAR || contentChars[start] == EXCLAMATION);
        }

        private static void ReconstructValidElementNode(ref IList<Node> nodes)
        {
            TextNode elementStart = (TextNode)nodes[0];
            TextNode elementBody = (TextNode)nodes[1];
            if (!char.IsLetter(elementBody.Text.ToCharArray()[0])) return;

            var firstSpaceAfterStart = elementBody.Text.IndexOf(SPACE);
            var elementWithoutAttributes = String.Format("{0}{1}/>", elementStart.Text, elementBody.Text.Substring(0, firstSpaceAfterStart));
            nodes = ParseNodes(elementWithoutAttributes);
        }

        private static string GetElementNodeAsString(ElementNode elementNode)
        {
            var attributes = string.Empty;
            elementNode.Attributes.ToList().ForEach(a => { attributes += string.Format("{0}=\"{1}\" ", a.Name, a.Value); });
            return String.Format("<{0} {1}/>", (elementNode).Name, attributes);
        }
        
        private static void GetFragmentStartAndEnd(string content, int position, out int start, out int end)
        {
            var elementStart = content.LastIndexOf(OPEN_ELEMENT, position > 0 ? position - 1 : 0);
            var expressionStart = content.LastIndexOf(OPEN_BRACE, position > 0 ? position - 1 : 0);
            bool isElement = elementStart > expressionStart;

            start = isElement ? elementStart : expressionStart - 1;
            var endChar = isElement ? CLOSE_ELEMENT : CLOSE_BRACE;
            end = content.LastIndexOf(endChar, position > 0 ? position - 1 : 0);
        }

        private static string GetOpeningFragment(string content, int position, int start)
        {
            var nextStart = content.IndexOf(OPEN_ELEMENT, position);

            var fullFragment = nextStart != -1
                ? content.Substring(start, nextStart - start)
                : content.Substring(start);

            CloseFragment(content, position, ref fullFragment);

            return fullFragment;
        }

        private static void CloseFragment(string content, int position, ref string fullFragment)
        {
            if (IsExpression(content, position))
            {
                if (!fullFragment.Contains(CLOSE_BRACE)) fullFragment += CLOSE_BRACE;
            }
            else if (!fullFragment.Contains(CLOSE_ELEMENT)) fullFragment += SELF_CLOSE_ELEMENT;
            else if (!fullFragment.Contains(SELF_CLOSE_ELEMENT)) fullFragment = fullFragment.Replace(CLOSE_ELEMENT.ToString(), SELF_CLOSE_ELEMENT);
        }

        private static bool IsPositionInElementName(string content, int position)
        {
            int start, end;
            if (IsElement(content, position))
            {
                GetFragmentStartAndEnd(content, position, out start, out end);
                return start != -1 && !content.Substring(start, position - start).Contains(SPACE);
            }
            return false;
        }

        private static bool IsPositionOutsideANode(int position, int start, int end)
        {
            return (end > start && end < position) || position == 0;
        }

        private static bool IsPositionInClosingElement(string content, int start)
        {
            return start < content.Length - 1 && content.ToCharArray()[start + 1] == FWD_SLASH;
        }

        private static Position Source(string content)
        {
            return new Position(new SourceContext(content));
        }

        private static IList<INodeVisitor> BuildNodeVisitors(VisitorContext context)
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

        private static IList<INodeVisitor> BuildChunkVisitors(VisitorContext context)
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
