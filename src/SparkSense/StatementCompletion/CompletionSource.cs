using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using SparkSense.Parsing;
using SparkSense.StatementCompletion.CompletionSets;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Operations;


namespace SparkSense.StatementCompletion
{
    public class CompletionSource : ICompletionSource
    {
        private bool _isDisposed;
        private ITextBuffer _textBuffer;
        private ITextStructureNavigator _textNavigator;

        public CompletionSource(ITextBuffer textBuffer, ITextStructureNavigator textNavigator)
        {
            _textBuffer = textBuffer;
            _textNavigator = textNavigator;
        }

        #region ICompletionSource Members

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            SparkSyntaxTypes syntaxType;
            IViewExplorer viewExplorer;
            ITextExplorer textExplorer;

            session.Properties.TryGetProperty(typeof(SparkSyntaxTypes), out syntaxType);
            session.Properties.TryGetProperty(typeof(ViewExplorer), out viewExplorer);
            session.Properties.TryGetProperty(typeof(TextExplorer), out textExplorer);
            
            CompletionSet sparkCompletions = SparkCompletionSetFactory.GetCompletionSetFor(session, _textBuffer, viewExplorer, syntaxType, textExplorer);
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