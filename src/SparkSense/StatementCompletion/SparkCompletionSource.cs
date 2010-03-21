using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

namespace SparkSense.StatementCompletion
{
    internal class SparkCompletionSource : ICompletionSource
    {
        private readonly SparkCompletionSourceFactory _sourceFactory;
        private readonly ITextBuffer _textBuffer;
        private bool _isDisposed;
        private readonly IEnumerable<Completion> _completionList;

        public SparkCompletionSource(SparkCompletionSourceFactory sourceFactory, ITextBuffer textBuffer)
        {
            _sourceFactory = sourceFactory;
            _textBuffer = textBuffer;

            _completionList = new List<Completion>
                                  {
                                      new Completion("Spark 'Use' Tag", "<use robWoz='ere' />", "Spark 'Use' Tag for stuffs and things", null, null)
                                  };
        }

        #region ICompletionSource Members

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            SnapshotPoint currentPosition = session.TextView.Caret.Position.BufferPosition - 1;
            ITextStructureNavigator navigator = _sourceFactory.NavigatorService.GetTextStructureNavigator(_textBuffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPosition);
            ITrackingSpan applicableTo = currentPosition.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);

            IEnumerable<Completion> completionBuilders = null;
            completionSets.Add(new CompletionSet("SparkTags", "Spark Tags", applicableTo, _completionList, completionBuilders));
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            GC.SuppressFinalize(this);
            _isDisposed = true;
        }

        #endregion
    }
}