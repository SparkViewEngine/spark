using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using SparkSense.Parsing;
using Microsoft.VisualStudio.Text.Editor;

namespace SparkSense.StatementCompletion
{
    public class CompletionSessionConfiguration : ICompletionSessionConfiguration
    {
        private ICompletionSession _session;
        private ITextView _textView;
        private ICompletionBroker _completionBroker;

        public CompletionSessionConfiguration(ICompletionBroker completionBroker, ITextView textView)
        {
            _completionBroker = completionBroker;
            _textView = textView;
        }


        public bool IsCompletionSessionActive()
        {
            return _completionBroker.IsCompletionActive(_textView);
        }

        public bool TryCreateCompletionSession(ITextExplorer textExplorer, out ICompletionSession completionSession)
        {
            _session = _completionBroker.CreateCompletionSession(textExplorer.TextView, textExplorer.GetTrackingPoint(), true);
            completionSession = _session;
            return completionSession != null;
        }

        public void AddCompletionSourceProperties(Dictionary<object, object> properties)
        {
            if (properties == null) return;
            foreach (var property in properties)
                _session.Properties.AddProperty(property.Key, property.Value);
        }
    }
}
