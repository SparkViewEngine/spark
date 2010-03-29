using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using SparkSense.StatementCompletion.CompletionSets;

namespace SparkSense.StatementCompletion
{
    internal class SparkCompletionSource : ICompletionSource
    {
        private readonly SparkCompletionSourceProvider _sourceProvider;
        private bool _isDisposed;
        private ITextBuffer _textBuffer;

        public SparkCompletionSource(SparkCompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
        {
            _sourceProvider = sourceProvider;
            _textBuffer = textBuffer;
        }

        protected SparkCompletionTypes CompletionType { get; private set; }

        protected ICompletionSession CurrentSession { get; private set; }

        #region ICompletionSource Members

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            SparkCompletionTypes completionType;
            if (!session.Properties.TryGetProperty(typeof (SparkCompletionTypes), out completionType))
                return;
            CompletionType = completionType;
            CurrentSession = session;

            _textBuffer = session.TextView.TextBuffer; //TODO: Rob G - Figure out why the session text buffer is different from the ctor text buffer
            SnapshotPoint completionStartPoint = session.GetTriggerPoint(_textBuffer).GetPoint(_textBuffer.CurrentSnapshot);
            CompletionSet sparkCompletions = GetCompletionSetFor(completionStartPoint);
            completionSets.Add(sparkCompletions);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            GC.SuppressFinalize(this);
            _isDisposed = true;
        }

        #endregion

        private CompletionSet GetCompletionSetFor(SnapshotPoint completionStartPoint)
        {
            switch (CompletionType)
            {
                case SparkCompletionTypes.Tag:
                    return SparkCompletionSetFactory.Create<SparkTagCompletionSet>(_sourceProvider, _textBuffer, completionStartPoint);
                case SparkCompletionTypes.Variable:
                    return SparkCompletionSetFactory.Create<SparkVariableCompletionSet>(_sourceProvider, _textBuffer, completionStartPoint);
                case SparkCompletionTypes.None:
                default:
                    return null;
            }
        }
    }
}