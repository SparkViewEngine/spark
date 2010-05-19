using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using SparkSense.Parsing;
using SparkSense.StatementCompletion.CompletionSets;


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

        #region ICompletionSource Members

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            SparkSyntaxTypes syntaxType;
            IViewExplorer viewExplorer;

            session.Properties.TryGetProperty(typeof(SparkSyntaxTypes), out syntaxType);
            session.Properties.TryGetProperty(typeof(ViewExplorer), out viewExplorer);

            CompletionSet sparkCompletions = SparkCompletionSetFactory.GetCompletionSetFor(session, _textBuffer, viewExplorer, syntaxType);
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