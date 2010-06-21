using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using SparkSense.Parsing;
using SparkSense.StatementCompletion.CompletionSets;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Operations;
using System.Diagnostics;


namespace SparkSense.StatementCompletion
{
    public class CompletionSource : ICompletionSource
    {
        private bool _isDisposed;
        private ITextBuffer _textBuffer;
        private IProjectExplorer _projectExplorer;
        private ITextStructureNavigator _textNavigator;

        public CompletionSource(ITextBuffer textBuffer, ITextStructureNavigator textNavigator, IProjectExplorer projectExplorer)
        {
            _textBuffer = textBuffer;
            _textNavigator = textNavigator;
            _projectExplorer = projectExplorer;
        }
        public CompletionSource(ITextBuffer textBuffer, ITextStructureNavigator textNavigator)
        {
            _textBuffer = textBuffer;
            _textNavigator = textNavigator;
        }

        #region ICompletionSource Members

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            //SparkSyntaxTypes syntaxType;
            //IViewExplorer viewExplorer;
            //ITextExplorer textExplorer;

            //session.Properties.TryGetProperty(typeof(SparkSyntaxTypes), out syntaxType);
            //session.Properties.TryGetProperty(typeof(ViewExplorer), out viewExplorer);
            //session.Properties.TryGetProperty(typeof(TextExplorer), out textExplorer);

            CompletionSet sparkCompletions = GetCompletionSetFor(session);
            if (sparkCompletions == null) return;

            MergeSparkWithAllCompletionsSet(completionSets, sparkCompletions);
            completionSets.Add(sparkCompletions);
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
            combinedList.Sort((a, b) => a.DisplayText.CompareTo(b.DisplayText));
            return combinedList;
        }

        public CompletionSet GetCompletionSetFor(ICompletionSession session)
        {
            var triggerPoint = session.GetTriggerPoint(_textBuffer).GetPoint(_textBuffer.CurrentSnapshot);
            var trackingSpan = triggerPoint.Snapshot.CreateTrackingSpan(new Span(triggerPoint, 0), SpanTrackingMode.EdgeInclusive);

            var syntax = new SparkSyntax(new TextExplorer(session.TextView, _textNavigator));
            var syntaxType = syntax.GetSyntaxType(_textBuffer.CurrentSnapshot[triggerPoint - 1]);

            Debug.WriteLine(string.Format("char '{0}' interpreted as '{1}'", _textBuffer.CurrentSnapshot[triggerPoint - 1], syntaxType));

            switch (syntaxType)
            {
                case SparkSyntaxTypes.Element:
                    return SparkCompletionSetFactory.Create<SparkTagCompletionSet>(session, _textBuffer, trackingSpan);
                case SparkSyntaxTypes.Attribute:
                    return null;
                case SparkSyntaxTypes.AttributeValue:
                    return null;
                case SparkSyntaxTypes.Variable:
                    return null;
                case SparkSyntaxTypes.Invalid:
                    return null;
                case SparkSyntaxTypes.None:
                default:
                    return null;
            }
        }

    }
}