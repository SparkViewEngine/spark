using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace SparkSense.StatementCompletion
{
    [Export(typeof (ICompletionSourceProvider))]
    [ContentType("spark")]
    [ContentType("HTML")]
    [Name("Spark Tag Completion")]
    public class CompletionListener : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService TextNavigator { get; set; }

        #region ICompletionSourceProvider Members

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new CompletionSource(TextNavigator, textBuffer);
        }

        #endregion
    }
}