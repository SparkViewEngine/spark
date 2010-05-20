using Microsoft.VisualStudio.Language.Intellisense;
using SparkSense.Parsing;
using System.Collections.Generic;

namespace SparkSense.StatementCompletion
{
    public class CompletionSessionConfiguration : ICompletionSessionConfiguration
    {
        private ICompletionSession _session;
        private ICompletionBroker _completionBroker;

        public CompletionSessionConfiguration(ICompletionBroker completionBroker)
        {
            _completionBroker = completionBroker;
        }
        
        public bool TryCreateCompletionSession(ITextExplorer textExplorer, out ICompletionSession completionSession)
        {
            _session = _completionBroker.CreateCompletionSession(textExplorer.TextView, textExplorer.GetTrackingPoint(), true);
            completionSession = _session;
            return completionSession != null;
        }

        public void AddCompletionSourceProperties(List<object> properties)
        {
            if (properties == null) return;
            properties.ForEach(property => _session.Properties.AddProperty(property.GetType(), property));
        }
    }
}
