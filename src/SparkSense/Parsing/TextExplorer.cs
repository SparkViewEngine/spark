
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Spark.Compiler.NodeVisitors;
using System.Collections.Generic;
using Spark.Parser;
using Spark.Parser.Markup;
using System;
using Microsoft.VisualStudio.Text.Operations;

namespace SparkSense.Parsing
{
    public class TextExplorer : ITextExplorer
    {
        private ITextStructureNavigator _textNavigator;

        public TextExplorer(ITextView textView)
        {
            TextView = textView;
        }

        public ITextView TextView { get; private set; }

        private static Position Source(string content)
        {
            return new Position(new SourceContext(content));
        }

        public ITrackingPoint GetTrackingPoint()
        {
            SnapshotPoint? caretPoint;
            if (!TryGetCaretPoint(out caretPoint)) return null;

            var trackingPoint = TextView.TextSnapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive);
            return trackingPoint;
        }

        public int GetStartPosition()
        {
            SnapshotPoint? caretPoint;
            return TryGetCaretPoint(out caretPoint) ? caretPoint.Value.Position : 0;
        }

        public ITrackingSpan GetTrackingSpan()
        {
            SnapshotPoint? caretPoint;
            return TryGetCaretPoint(out caretPoint)
                ? TextView.TextSnapshot.CreateTrackingSpan(caretPoint.Value.Position, 0, SpanTrackingMode.EdgeInclusive)
                : null;
        }

        private bool TryGetCaretPoint(out SnapshotPoint? caretPoint)
        {
            caretPoint = null;
            if (TextView == null || TextView.Caret == null) return false;

            caretPoint = TextView.Caret.Position.BufferPosition;
            return true;
        }

        public IList<Node> GetParsedNodes()
        {
            string content = TextView.TextSnapshot.GetText();
            return GetParsedNodes(content);
        }

        public IList<Node> GetParsedNodes(string content)
        {
            var grammar = new MarkupGrammar();
            var result = grammar.Nodes(Source(content));
            var nodes = result.Value;

            foreach (var visitor in BuildNodeVisitors(new VisitorContext()))
            {
                visitor.Accept(nodes);
                nodes = visitor.Nodes;
            }

            return nodes;
        }

        public string GetTagAtPosition(int position)
        {
            var content = TextView.TextSnapshot.GetText();
            int tagStart = content.LastIndexOf('<', position - 1);
            int tagClose = content.IndexOf('>', tagStart);
            var nextTag = content.IndexOf('<', tagStart + 1);

            var tagIsClosed = tagClose < nextTag && nextTag != -1;
            var tag = content.Substring(tagStart, tagIsClosed ? tagClose - tagStart + 1 : nextTag - tagStart);
            if (!tagIsClosed)
                tag += "/>";
            return tag;
        }

        public Node GetNodeAtPosition(int position)
        {
            string tag = GetTagAtPosition(position);
            var grammar = new MarkupGrammar();
            var node = grammar.AnyNode(Source(tag)).Value;

            return node;
        }

        public string GetCurrentWord()
        {
            var point = TextView.Caret.Position.Point.GetPoint(TextView.TextBuffer, Microsoft.VisualStudio.Text.PositionAffinity.Predecessor);
            var currentWord = _textNavigator.GetExtentOfWord(point.Value - 1);
            return currentWord.Span.GetText();
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
