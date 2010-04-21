using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using SparkSense.StatementCompletion.CompletionSets;
using System.Collections;

namespace SparkSense.StatementCompletion
{
    internal class SparkCompletionSource : ICompletionSource
    {
        private readonly SparkCompletionSourceProvider _sourceProvider;
        private bool _isDisposed;
        private ITextBuffer _textBuffer;
        private List<CompletionSet> _completionSetsToInclude = new List<CompletionSet>();

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
            if (!session.Properties.TryGetProperty(typeof(SparkCompletionTypes), out completionType))
            {
                _completionSetsToInclude.AddRange(completionSets);
                completionSets.Clear();
                return;
            }
            CompletionType = completionType;
            CurrentSession = session;

            SnapshotPoint completionStartPoint = session.GetTriggerPoint(_textBuffer).GetPoint(_textBuffer.CurrentSnapshot);
            CompletionSet sparkCompletions = GetCompletionSetFor(completionStartPoint);
            CombineCompletionSets(sparkCompletions);
            completionSets.Add(sparkCompletions);
        }

        private void CombineCompletionSets(CompletionSet sparkCompletions)
        {
            var combinedCompletions = new List<Completion>();

            _completionSetsToInclude.ForEach(cs => combinedCompletions.AddRange(cs.Completions));
            combinedCompletions.AddRange(sparkCompletions.Completions);

            combinedCompletions.Sort((cm1, cm2) => cm1.DisplayText.CompareTo(cm2.DisplayText));

            sparkCompletions.Completions.Clear();

            combinedCompletions.ForEach(completion => sparkCompletions.Completions.Add(completion));

            _completionSetsToInclude.Clear();
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