using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using SparkSense.Parsing;

namespace SparkSense.StatementCompletion
{
    public interface ICompletionSessionConfiguration
    {
        bool TryCreateCompletionSession(ITextExplorer textExplorer, out ICompletionSession completionSession);
        void AddCompletionSourceProperties(Dictionary<object, object> properties);
    }
}
