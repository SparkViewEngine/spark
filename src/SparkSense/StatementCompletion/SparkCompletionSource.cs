using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

namespace SparkSense.StatementCompletion
{
    internal class SparkCompletionSource : ICompletionSource
    {
        private readonly SparkCompletionSourceProvider _sourceProvider;
        private readonly ITextBuffer _textBuffer;
        private bool _isDisposed;
        private readonly IEnumerable<Completion> _completionList;
        private ImageSource _sparkIcon;

        public SparkCompletionSource(SparkCompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
        {
            _sourceProvider = sourceProvider;
            _textBuffer = textBuffer;

            _completionList = new List<Completion>
                                  {
                                      new Completion("<content", "<content", "Spark 'content' tag for spooling output to various text writers", SparkIcon, null),
                                      new Completion("<default", "<default", "Spark 'default' tag for declaring local variables if a symbol of a given name is not known to be in scope", SparkIcon, null),
                                  };
        }

        protected ImageSource SparkIcon
        {
            get { return _sparkIcon ?? (_sparkIcon = new BitmapImage(new Uri(("Resources/SparkTag.png"), UriKind.Relative))); }
        }

        #region ICompletionSource Members

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            SnapshotPoint currentPosition = session.TextView.Caret.Position.BufferPosition - 1;
            ITextStructureNavigator navigator = _sourceProvider.NavigatorService.GetTextStructureNavigator(_textBuffer);
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