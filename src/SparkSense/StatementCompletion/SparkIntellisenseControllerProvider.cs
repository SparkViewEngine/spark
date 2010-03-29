using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace SparkSense.StatementCompletion
{
    //[Export(typeof(IIntellisenseControllerProvider))]
    [Name("Spark Intellisense Controller")]
    [Order(Before = Priority.Default)]
    [ContentType("text")]
    [ContentType("HTML")]
    internal class SparkIntellisenseControllerProvider : IIntellisenseControllerProvider
    {
        [Import] internal IVsEditorAdaptersFactoryService AdaptersFactoryService;

        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }

        #region IIntellisenseControllerProvider Members

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> textBuffers)
        {
            return textBuffers.Where(IsSparkSyntax).Select(
                textBuffer => new SparkIntellisenseController(this, textBuffer, textView)).FirstOrDefault();
        }

        #endregion

        private static bool IsSparkSyntax(ITextBuffer buffer)
        {
            return buffer.ContentType.TypeName == "text" ||
                   buffer.ContentType.TypeName == "HTML";
        }
    }
}