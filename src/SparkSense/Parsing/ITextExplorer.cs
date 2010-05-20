using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace SparkSense.Parsing
{
    public interface ITextExplorer
    {
        ITextView TextView { get; }
        ITrackingPoint GetTrackingPoint();
        int GetStartPosition();
        ITrackingSpan GetTrackingSpan();
    }
}