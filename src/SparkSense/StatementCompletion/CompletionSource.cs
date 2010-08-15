using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Spark.Parser.Markup;
using SparkSense.Parsing;
using SparkSense.StatementCompletion.CompletionSets;
using System;
using System.Collections.Generic;

namespace SparkSense.StatementCompletion
{
    public class CompletionSource : ICompletionSource
    {
        private bool _isDisposed;
        private ITextBuffer _textBuffer;
        private IProjectExplorer _projectExplorer;
        private IViewExplorer _viewExplorer;
        private ITrackingSpan _trackingSpan;

        public CompletionSource(ITextBuffer textBuffer, IProjectExplorer projectExplorer)
        {
            _textBuffer = textBuffer;
            _projectExplorer = projectExplorer;
            _viewExplorer = _projectExplorer.GetViewExplorer(textBuffer);
            _textBuffer.Changed += TextBuffer_Changed;
        }

        void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            if (e.After != _textBuffer.CurrentSnapshot)
                return;
            _viewExplorer.InvalidateView(_textBuffer.CurrentSnapshot.GetText());
        }
        
        #region ICompletionSource Members

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var triggerPoint = session.GetTriggerPoint(_textBuffer).GetPoint(_textBuffer.CurrentSnapshot);

            if (!session.Properties.TryGetProperty(typeof(ITrackingSpan), out _trackingSpan))
                _trackingSpan = triggerPoint.Snapshot.CreateTrackingSpan(new Span(triggerPoint, 0), SpanTrackingMode.EdgeInclusive);

            CompletionSet sparkCompletions = CompletionSetFactory.GetCompletionSetFor(triggerPoint, _trackingSpan, _viewExplorer);
            if (sparkCompletions == null) return;

            MergeSparkWithAllCompletionsSet(completionSets, sparkCompletions);
            completionSets.Add(sparkCompletions);

            session.Committed += session_Committed;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            GC.SuppressFinalize(this);
            _isDisposed = true;
        }

        #endregion

        private static void MergeSparkWithAllCompletionsSet(IList<CompletionSet> completionSets, CompletionSet sparkCompletions)
        {

            CompletionSet allCompletionsSet;
            if (!TryExtractAllCompletionsSet(completionSets, out allCompletionsSet)) return;

            var mergedCompletionSet = new CompletionSet(
                                allCompletionsSet.Moniker,
                                allCompletionsSet.DisplayName,
                                allCompletionsSet.ApplicableTo,
                                GetCombinedSortedList(sparkCompletions, allCompletionsSet),
                                allCompletionsSet.CompletionBuilders);

            completionSets.Remove(allCompletionsSet);
            completionSets.Add(mergedCompletionSet);
        }

        private static bool TryExtractAllCompletionsSet(IList<CompletionSet> completionSets, out CompletionSet allCompletions)
        {
            allCompletions = null;
            foreach (var completionSet in completionSets)
            {
                if (completionSet.DisplayName != "All") continue;
                allCompletions = completionSet;
                return true;
            }
            return false;
        } 

        private static List<Completion> GetCombinedSortedList(CompletionSet sparkCompletions, CompletionSet allCompletionsSet)
        {
            var combinedList = new List<Completion>();
            combinedList.AddRange(allCompletionsSet.Completions);
            combinedList.AddRange(sparkCompletions.Completions);
            return combinedList.SortAlphabetically();
        }

        private bool IsCompletionChar(ICompletionSession session, char completionChar)
        {
            var point = session.TextView.Caret.Position.BufferPosition;
            return point.Position > 1 && (point - 1).GetChar() == completionChar;
        }

        void session_Committed(object sender, EventArgs e)
        {
            var session = sender as ICompletionSession;
            if (session == null || session.IsDismissed) return;
            if (!IsCompletionChar(session, Constants.DOUBLE_QUOTE)
                && !IsCompletionChar(session, Constants.SINGLE_QUOTE)) return;

            session.TextView.Caret.MoveToPreviousCaretPosition();
        }

    }
}