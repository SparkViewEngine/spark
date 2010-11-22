using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.Parser;
using Spark.Parser.Markup;
using System;
using System.Collections.Generic;
using System.Linq;
using Spark.Parser.Syntax;

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
            return nodes.Count == 2 && nodes[0] is TextNode && ((TextNode)nodes[0]).Text == Constants.OPEN_ELEMENT.ToString();
        }

        public static Type ParseContext(string content, int position)
        {
            var contentChars = content.ToCharArray();
            var previousChar = position > 0 ? contentChars[position - 1] : char.MinValue;
            switch (previousChar)
            {
                case Constants.OPEN_ELEMENT:
                case Constants.COLON:
                    return typeof(ElementNode);
                case Constants.SPACE:
                case Constants.DOUBLE_QUOTE:
                case Constants.SINGLE_QUOTE:
                    return typeof(AttributeNode);
                case Constants.OPEN_BRACE:
                case Constants.PERIOD:
                    if (IsExpression(content, position))
                        return typeof(ExpressionNode);
                    break;
                default:
                    break;
            }

            if (IsPositionInElementName(content, position))
                return typeof(ElementNode);
            if (IsPositionInAttribute(content, position))
                return typeof(AttributeNode);

            return typeof(TextNode);
        }

        public static IList<Chunk> ParseElementChunks(string content, int position)
        {
            var node = ParseNode(content, position);
            var contentToParse = node is ElementNode ? GetElementNodeAsString((ElementNode)node) : content;

            var grammar = new MarkupGrammar();
            var visitorContext = new VisitorContext { SyntaxProvider = new DefaultSyntaxProvider(new ParserSettings()) };
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
            return chunks;
        }

        public static Type ParseContextChunk(string content, int position)
        {
            IList<Chunk> chunks = ParseElementChunks(content, position);
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

        public static bool IsSpecialNode(Node currentNode, out Node sparkNode)
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

        public static bool IsPositionInAttributeName(string content, int position)
        {
            if (!IsElement(content, position)) return false;

            var startToCaret = GetStartToCaret(content, position);
            int lastEquals = GetLastEquals(startToCaret);
            if (lastEquals == -1) return true;
            var lastSpace = startToCaret.LastIndexOf(Constants.SPACE);
            var lastOpenQuote = startToCaret.IndexOfAny(new char[] { Constants.DOUBLE_QUOTE, Constants.SINGLE_QUOTE }, lastEquals);
            var lastQuote = startToCaret.LastIndexOfAny(new char[] { Constants.DOUBLE_QUOTE, Constants.SINGLE_QUOTE });

            if (lastQuote == lastOpenQuote && lastQuote != -1) return false;
            return lastEquals != -1 && lastSpace != -1 && lastSpace > lastEquals;
        }

        public static bool IsPositionInAttributeValue(string content, int position)
        {
            if (!IsElement(content, position)) return false;

            var startToCaret = GetStartToCaret(content, position);
            int lastEquals = GetLastEquals(startToCaret);
            if (lastEquals == -1) return false;
            var lastOpenQuote = startToCaret.IndexOfAny(new char[] { Constants.DOUBLE_QUOTE, Constants.SINGLE_QUOTE }, lastEquals);
            var lastQuote = startToCaret.LastIndexOfAny(new char[] { Constants.DOUBLE_QUOTE, Constants.SINGLE_QUOTE });
            return lastQuote == lastOpenQuote && lastQuote != -1;
        }

        private static int GetLastEquals(string startToCaret)
        {
            var lastEquals = startToCaret.LastIndexOf(Constants.EQUALS.ToString() + Constants.DOUBLE_QUOTE.ToString());
            lastEquals = lastEquals == -1 ? startToCaret.LastIndexOf(Constants.EQUALS.ToString() + Constants.SINGLE_QUOTE.ToString()) : lastEquals;
            return lastEquals == -1 ? startToCaret.LastIndexOf(Constants.EQUALS.ToString()) : lastEquals;
        }

        private static bool IsElement(string content, int position)
        {
            int start, end;
            GetFragmentStartAndEnd(content, position, out start, out end);
            return
                start > -1 &&
                !IsPositionOutsideANode(position, start, end) &&
                content.ToCharArray()[start] == Constants.OPEN_ELEMENT;
        }

        public static bool IsExpression(string content, int position)
        {
            int start, end;
            GetFragmentStartAndEnd(content, position, out start, out end);

            var contentChars = content.ToCharArray();
            return start > -1 &&
                (contentChars[start] == Constants.DOLLAR || contentChars[start] == Constants.EXCLAMATION);
        }

        private static void ReconstructValidElementNode(ref IList<Node> nodes)
        {
            TextNode elementStart = (TextNode)nodes[0];
            TextNode elementBody = (TextNode)nodes[1];
            if (!char.IsLetter(elementBody.Text.ToCharArray()[0])) return;

            var firstSpaceAfterStart = elementBody.Text.IndexOf(Constants.SPACE);
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
            var elementStart = content.LastIndexOf(Constants.OPEN_ELEMENT, position > 0 ? position - 1 : 0);
            var expressionStart = content.LastIndexOf(Constants.OPEN_BRACE, position > 0 ? position - 1 : 0);
            bool isElement = elementStart > expressionStart;

            start = isElement ? elementStart : expressionStart - 1;
            var endChar = isElement ? Constants.CLOSE_ELEMENT : Constants.CLOSE_BRACE;
            end = content.LastIndexOf(endChar, position > 0 ? position - 1 : 0);
            if (end < start) end = content.Length;
        }

        private static string GetOpeningFragment(string content, int position, int start)
        {
            var nextStart = content.IndexOf(Constants.OPEN_ELEMENT, position);

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
                if (!fullFragment.Contains(Constants.CLOSE_BRACE)) fullFragment += Constants.CLOSE_BRACE;
            }
            else if (!fullFragment.Contains(Constants.CLOSE_ELEMENT)) fullFragment += Constants.SELF_CLOSE_ELEMENT;
            else if (!fullFragment.Contains(Constants.SELF_CLOSE_ELEMENT)) fullFragment = fullFragment.Replace(Constants.CLOSE_ELEMENT.ToString(), Constants.SELF_CLOSE_ELEMENT);
        }

        private static bool IsPositionInElementName(string content, int position)
        {
            int start, end;
            if (IsElement(content, position))
            {
                GetFragmentStartAndEnd(content, position, out start, out end);
                return start != -1 && !content.Substring(start, position - start).Contains(Constants.SPACE);
            }
            return false;
        }

        private static bool IsPositionOutsideANode(int position, int start, int end)
        {
            return (end > start && end < position) || position == 0;
        }

        private static bool IsPositionInClosingElement(string content, int start)
        {
            return start < content.Length - 1 && content.ToCharArray()[start + 1] == Constants.FWD_SLASH;
        }

        private static bool IsPositionInAttribute(string content, int position)
        {
            return IsPositionInAttributeName(content, position) || IsPositionInAttributeValue(content, position);
        }

        private static string GetStartToCaret(string content, int position)
        {
            Position startPosition = GetStartPosition(content, position);
            var caretPosition = startPosition.Advance(position - startPosition.Offset);
            var attributeBeforeCaret = startPosition.Advance(startPosition.PotentialLength(Constants.SPACE) + 1).Constrain(caretPosition);
            var startToCaret = attributeBeforeCaret.Peek(attributeBeforeCaret.PotentialLength());
            return startToCaret;
        }
        
        private static Position GetStartPosition(string content, int position)
        {
            var parser = new MarkupGrammar();
            Position startPosition = null;
            var endPosition = Source(content);
            ParseResult<Node> currentNode = null;
            while (endPosition.Offset < position)
            {
                startPosition = endPosition;
                currentNode = parser.AnyNode(startPosition);
                endPosition = currentNode.Rest;
            }
            return startPosition;
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
                           new WhitespaceCleanerVisitor(context),
                           new ForEachAttributeVisitor(context),
                           new ConditionalAttributeVisitor(context),
                           new TestElseElementVisitor(context),
                           new OnceAttributeVisitor(context),
                           new UrlAttributeVisitor(context),
                           //new BindingExpansionVisitor(context)
                       };
        }

    }
}
