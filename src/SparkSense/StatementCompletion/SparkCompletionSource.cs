using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using SparkSense.StatementCompletion.CompletionSets;

namespace SparkSense.StatementCompletion
{
    internal class SparkCompletionSource : ICompletionSource
    {
        private readonly SparkCompletionSourceProvider _sourceProvider;
        private readonly ITextBuffer _textBuffer;
        private bool _isDisposed;
        private readonly IEnumerable<Completion> _completionList;
        private ImageSource _sparkTagIcon;

        public SparkCompletionSource(SparkCompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
        {
            _sourceProvider = sourceProvider;
            _textBuffer = textBuffer;
        }

        #region ICompletionSource Members

        protected SparkCompletionTypes CompletionType { get; private set; }

        protected ICompletionSession CurrentSession { get; private set; }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            SparkCompletionTypes completionType;
            if (!session.Properties.TryGetProperty(typeof(SparkCompletionTypes), out completionType))
                return;
            CompletionType = completionType;
            CurrentSession = session;

            //SnapshotPoint currentPosition = session.GetTriggerPoint(_textBuffer).GetPoint(_textBuffer.CurrentSnapshot);
            SnapshotPoint currentPosition = session.TextView.Caret.Position.BufferPosition - 1;
            CompletionSet sparkCompletions = GetCompletionSetFor(currentPosition);
            completionSets.Add(sparkCompletions);
        }

        private CompletionSet GetCompletionSetFor(SnapshotPoint currentPosition)
        {
            switch (CompletionType)
            {
                case SparkCompletionTypes.Tag:
                    return SparkCompletionSetFactory.Create<SparkTagCompletionSet>(_sourceProvider, _textBuffer, currentPosition);
                case SparkCompletionTypes.Variable:
                    return SparkCompletionSetFactory.Create<SparkVariableCompletionSet>(_sourceProvider, _textBuffer, currentPosition);
                case SparkCompletionTypes.None:
                default:
                    return null;
            }
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