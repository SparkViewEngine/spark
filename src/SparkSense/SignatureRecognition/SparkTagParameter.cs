using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace SparkSense.SignatureRecognition
{
    internal class SparkTagParameter : IParameter
    {
        public SparkTagParameter(string documentation, Span span, string name, ISignature signature)
        {
            Documentation = documentation;
            Locus = span;
            Name = name;
            Signature = signature;
        }

        #region IParameter Members

        public ISignature Signature { get; private set; }
        public string Name { get; private set; }
        public string Documentation { get; private set; }
        public Span Locus { get; private set; }
        public Span PrettyPrintedLocus { get; private set; }

        #endregion
    }
}