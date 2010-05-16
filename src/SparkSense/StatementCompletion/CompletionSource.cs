using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using SparkSense.StatementCompletion.CompletionSets;
using SparkSense.Parsing;
using Microsoft.VisualStudio.Text.Operations;

namespace SparkSense.StatementCompletion
{
    internal class CompletionSource : ICompletionSource
    {
        private readonly ITextStructureNavigatorSelectorService _textNavigator;
        private bool _isDisposed;
        private ITextBuffer _textBuffer;

        public CompletionSource(ITextStructureNavigatorSelectorService textNavigator, ITextBuffer textBuffer)
        {
            _textNavigator = textNavigator;
            _textBuffer = textBuffer;
        }

        protected CompletionTypes CompletionType { get; private set; }

        #region ICompletionSource Members

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            CompletionTypes completionType;
            IViewExplorer viewExplorer;

            session.Properties.TryGetProperty(typeof(CompletionTypes), out completionType);
            session.Properties.TryGetProperty(typeof(IViewExplorer), out viewExplorer);

            CompletionSet sparkCompletions = SparkCompletionSetFactory.GetCompletionSetFor(session, _textBuffer, viewExplorer, completionType);
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