using Microsoft.VisualStudio.Language.Intellisense;
using SparkSense.Parsing;
using System.Collections.Generic;

namespace SparkSense.StatementCompletion
{
    public interface ICompletionSessionConfiguration
    {
        bool TryCreateCompletionSession(ITextExplorer textExplorer, out ICompletionSession completionSession);
        void AddCompletionSourceProperties(List<object> properties);
    }
}
