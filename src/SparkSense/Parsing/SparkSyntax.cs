using System;
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
            return nodes.Count == 2 && nodes[0] is TextNode && ((TextNode)nodes[0]).Text == "<";
        }

        public static Type ParseContext(string content, int position)
        {
            var contentChars = content.ToCharArray();
            var previousChar = contentChars[position - 1];
            switch (previousChar)
            {
                case '<':
                    return typeof(ElementNode);
                case ' ':
                    return typeof(AttributeNode);
                case '{':
                    if (IsExpression(content, position))
                        return typeof(ExpressionNode);
                    break;
                case ':':
                    return ParseChunkContext(content, position);
                default:
                    break;
            }

            if (IsPositionInElementName(content, position))
                return typeof(ElementNode);

            return typeof(TextNode);
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
                content.ToCharArray()[start] == '<';
        }

        private static bool IsExpression(string content, int position)
        {
            int start, end;
            GetFragmentStartAndEnd(content, position, out start, out end);

            var contentChars = content.ToCharArray();
            return start > -1 && 
                (contentChars[start] == '$' || contentChars[start] == '!');
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

        private static Type ParseChunkContext(string content, int position)
        {
            var node = ParseNode(content, position);
            var contentToParse = node is ElementNode ? String.Format("<{0} />", ((ElementNode)node).Name) : content;
            
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
        
        private static void GetFragmentStartAndEnd(string content, int position, out int start, out int end)
        {
            var elementStart = content.LastIndexOf('<', position > 0 ? position - 1 : 0);
            var expressionStart = content.LastIndexOf('{', position > 0 ? position - 1 : 0);
            bool isElement = elementStart > expressionStart;

            start = isElement ? elementStart : expressionStart - 1;
            var endChar = isElement ? '>' : '}';
            end = content.LastIndexOf(endChar, position > 0 ? position - 1 : 0);
        }

        private static string GetOpeningFragment(string content, int position, int start)
        {
            var nextStart = content.IndexOf('<', position);

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
                if (!fullFragment.Contains("}")) fullFragment += "}";
            }
            else if (!fullFragment.Contains(">")) fullFragment += "/>";
            else if (!fullFragment.Contains("/>")) fullFragment = fullFragment.Replace(">", "/>");
        }

        private static bool IsPositionInElementName(string content, int position)
        {
            int start, end;
            if (IsElement(content, position))
            {
                GetFragmentStartAndEnd(content, position, out start, out end);
                return start != -1 && !content.Substring(start, position - start).Contains(" ");
            }
            return false;
        }

        private static bool IsPositionOutsideANode(int position, int start, int end)
        {
            return (end > start && end < position) || position == 0;
        }

        private static bool IsPositionInClosingElement(string content, int start)
        {
            return start < content.Length - 1 && content.ToCharArray()[start + 1] == '/';
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
