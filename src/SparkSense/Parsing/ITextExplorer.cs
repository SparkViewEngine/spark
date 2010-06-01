using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Spark.Parser.Markup;
using System.Collections.Generic;

namespace SparkSense.Parsing
{
    public interface ITextExplorer
    {
        ITextView TextView { get; }
        ITrackingPoint GetTrackingPoint();
        int GetStartPosition();
        ITrackingSpan GetTrackingSpan();
        IList<Node> GetParsedNodes();
        bool IsCaretContainedWithinTag();
        Node GetNodeAtPosition(int position);
    }
}