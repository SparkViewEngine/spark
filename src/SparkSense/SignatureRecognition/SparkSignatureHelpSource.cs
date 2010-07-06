using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace SparkSense.SignatureRecognition
{
    internal class SparkSignatureHelpSource : ISignatureHelpSource
    {
        private readonly ITextBuffer _textBuffer;
        private bool _isDisposed;

        public SparkSignatureHelpSource(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
        }

        #region ISignatureHelpSource Members

        public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
        {
            ITextSnapshot currentSnapshot = _textBuffer.CurrentSnapshot;
            int currentPosition = session.GetTriggerPoint(_textBuffer).GetPosition(currentSnapshot);
            ITrackingSpan currentApplicableSpan = currentSnapshot.CreateTrackingSpan(new Span(currentPosition, 0), SpanTrackingMode.EdgeInclusive, TrackingFidelityMode.Forward);

            signatures.Add(CreateSignature(_textBuffer, "<content name=\"namedContentArea\" />", "Spools all output in the content element into a named text writer", currentApplicableSpan));
            signatures.Add(CreateSignature(_textBuffer, "<content var=\"variable\" />", "Spools all output into a temporary text writer", currentApplicableSpan));
            signatures.Add(CreateSignature(_textBuffer, "<content def=\"variable\" />", "Spools all output into a temporary text writer (same as 'var')", currentApplicableSpan));
            signatures.Add(CreateSignature(_textBuffer, "<default x=\"xValue\" y=\"yValue\" />", "Declares local variables if a symbol of a given name is not known to be in scope", currentApplicableSpan));
        }

        public ISignature GetBestMatch(ISignatureHelpSession session)
        {
            if (session.Signatures.Count <= 0) return null;

            //TODO: Rob G - Flesh this out to cover various signatures

            return session.Signatures[0];
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            GC.SuppressFinalize(this);
            _isDisposed = true;
        }

        #endregion

        private static SparkTagSignature CreateSignature(ITextBuffer textBuffer, string tagSignature, string tagDocumentation, ITrackingSpan currentApplicableSpan)
        {
            var signature = new SparkTagSignature(textBuffer, tagSignature, tagDocumentation, null);
            textBuffer.Changed += signature.OnTextBufferChanged;

            string[] parameters = tagSignature.Split(' ');
            IList<IParameter> paramList = new List<IParameter>();

            int spanSearchStart = 0;
            for (int i = 1; i < parameters.Length; i++)
            {
                string param = parameters[i].Trim();
                if (string.IsNullOrEmpty(param)) continue;

                int spanStart = tagSignature.IndexOf(param, spanSearchStart);
                if (spanStart >= 0)
                {
                    var span = new Span(spanStart, param.Length);
                    spanSearchStart = spanStart + param.Length;
                    IParameter parameter = new SparkTagParameter("param doc", span, param, signature);
                    paramList.Add(parameter);
                }
            }

            signature.Parameters = new ReadOnlyCollection<IParameter>(paramList);
            signature.ApplicableToSpan = currentApplicableSpan;
            signature.InitCurrentParameter();
            return signature;
        }
    }
}