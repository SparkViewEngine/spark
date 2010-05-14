using EnvDTE;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Syntax;
using System;
using System.Collections.Generic;
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
            CompletionType = session.Properties.TryGetProperty(typeof(SparkCompletionTypes), out completionType) ? completionType : SparkCompletionTypes.None;
            CurrentSession = session;

            SnapshotPoint completionStartPoint = session.GetTriggerPoint(_textBuffer).GetPoint(_textBuffer.CurrentSnapshot);
            CompletionSet sparkCompletions = SparkCompletionSetFactory.GetCompletionSetFor(_textBuffer, completionStartPoint, completionType);
            if (sparkCompletions != null) completionSets.Add(sparkCompletions);
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