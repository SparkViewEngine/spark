
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace SparkSense.Parsing
{
    public class TextExplorer : ITextExplorer
    {
        public TextExplorer(ITextView textView)
        {
            TextView = textView;
        }

        public ITextView TextView { get; private set; }

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
    }
}
