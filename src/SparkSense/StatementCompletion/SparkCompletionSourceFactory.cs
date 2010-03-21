using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace SparkSense.StatementCompletion
{
    [Export(typeof (ICompletionSourceProvider))]
    [ContentType("spark")]
    [Name("Spark Tag Completion")]
    internal class SparkCompletionSourceFactory : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        #region ICompletionSourceProvider Members

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new SparkCompletionSource(this, textBuffer);
        }

        #endregion
    }
}