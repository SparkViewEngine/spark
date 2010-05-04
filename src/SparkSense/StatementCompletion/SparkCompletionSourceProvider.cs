using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace SparkSense.StatementCompletion
{
    [Export(typeof (ICompletionSourceProvider))]
    [ContentType("spark")]
    [ContentType("HTML")]
    [Name("Spark Tag Completion")]
    public class SparkCompletionSourceProvider : ICompletionSourceProvider
    {
        private ITextStructureNavigatorSelectorService _navigatorService;
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService
        {
            get
            {
                return _navigatorService;
            }
            set
            {
                _navigatorService = value;
            }
        }

        #region ICompletionSourceProvider Members

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new SparkCompletionSource(this, textBuffer);
        }

        #endregion
    }
}