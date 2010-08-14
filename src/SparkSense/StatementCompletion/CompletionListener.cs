using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using SparkSense.Parsing;
using System;
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
            IProjectExplorer projectExplorer = ServiceProvider.VsEnvironment != null ? new ProjectExplorer(ServiceProvider) : null;
            return projectExplorer != null ? new CompletionSource(textBuffer, projectExplorer) : null;
        }

        #endregion
    }
}