using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace SparkSense.Parsing
{
    public interface ITextExplorer
    {
        ITextView TextView { get; }
        ITrackingPoint TriggerPoint { get; }
    }

    public class TextExplorer : ITextExplorer
    {
        public TextExplorer(ITextView textView, ITrackingPoint triggerPoint)
        {
            TextView = textView;
            TriggerPoint = triggerPoint;
        }
        public ITextView TextView { get; private set; }
        public ITrackingPoint TriggerPoint { get; private set; }
    }
}