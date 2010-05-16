using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace SparkSense.SignatureRecognition
{
    [Export(typeof (ISignatureHelpSourceProvider))]
    [Name("Spark Tag Signature Help Source")]
    [ContentType("spark")]
    [ContentType("HTML")]
    internal class SparkSignatureHelpSourceProvider : ISignatureHelpSourceProvider
    {
        #region ISignatureHelpSourceProvider Members

        public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer)
        {
            return null; //temporary
            //return new SparkSignatureHelpSource(textBuffer);
        }

        #endregion
    }
}