using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace SparkSense.StatementCompletion
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("spark")]
    [ContentType("HTML")]
    [Name("Spark Tag Completion")]
    public class CompletionListener : ICompletionSourceProvider
    {
        [Import]
        public ISparkServiceProvider ServiceProvider { get; set; }

        #region ICompletionSourceProvider Members

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return ServiceProvider.ProjectExplorer != null ? new CompletionSource(textBuffer, ServiceProvider.ProjectExplorer) : null;
        }

        #endregion
    }
}