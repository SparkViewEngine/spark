using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SparkSense.SignatureRecognition
{
    [Export(typeof (ISignatureHelpSourceProvider))]
    [Name("Spark Tag Signature Help Source")]
    [Order(Before = Priority.Default)]
    [ContentType("spark")]
    internal class SparkSignatureHelpSourceProvider : ISignatureHelpSourceProvider
    {
        #region ISignatureHelpSourceProvider Members

        public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer)
        {
            return null;
            //return new SparkSignatureHelpSource(textBuffer);
        }

        #endregion
    }
}