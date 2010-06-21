using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using SparkSense.Parsing;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text;

namespace SparkSense.StatementCompletion
{
    public class CompletionSessionManager
    {
        private ICompletionSessionConfiguration _config;
        private IProjectExplorer _projectExplorer;
        private IWpfTextView _textView;
        private ICompletionSession _sparkOnlySession;
        private ITextExplorer _textExplorer;
        private ITextStructureNavigator _textNavigator;

        public CompletionSessionManager(ICompletionSessionConfiguration config, IProjectExplorer projectExplorer, IWpfTextView textView, ITextStructureNavigator textNavigator)
        {
            if (config == null) throw new ArgumentNullException("config", "Session Config is null.");
            if (projectExplorer == null) throw new ArgumentNullException("projectExplorer", "Project Explorer is null.");
            if (textView == null) throw new ArgumentNullException("textView", "Text View is null.");
            if (textNavigator == null) throw new ArgumentNullException("textNavigator", "textNavigator is null.");

            _config = config;
            _projectExplorer = projectExplorer;
            _textView = textView;
            _textNavigator = textNavigator;
            _textExplorer = new TextExplorer(_textView, _textNavigator);
        }

        public bool IsSparkOnlySessionActive()
        {
            return _sparkOnlySession != null && !_sparkOnlySession.IsDismissed;
        }

        public bool IsCompletionCommitted(uint key, char inputCharacter)
        {
            if (!(key.IsACommitCharacter(inputCharacter) && IsSparkOnlySessionActive()))
                return false;

            if (_sparkOnlySession.SelectedCompletionSet.SelectionStatus.IsSelected)
            {
                _sparkOnlySession.Commit();
                return true;
            }
            _sparkOnlySession.Dismiss();
            return false;
        }

        public bool IsCompletionStarted(uint key, char inputCharacter)
        {
            bool active = _config.IsCompletionSessionActive();

            if (active) return true;

            SparkSyntaxTypes syntaxType;
            if (!TryEvaluateSparkSyntax(inputCharacter, out syntaxType))
                return IsMovementOrDeletionHandled(key);

            if (IsSparkOnlySessionActive() || StartCompletionSession(syntaxType))
                _sparkOnlySession.Filter();
            return true;
        }

        private bool TryEvaluateSparkSyntax(char inputCharacter, out SparkSyntaxTypes syntaxType)
        {
            var sparkSyntax = new SparkSyntax(_textExplorer);
            syntaxType = _projectExplorer.ViewFolderExists()
                ? SparkSyntaxTypes.None
                : SparkSyntaxTypes.Invalid;

            return _projectExplorer.IsCurrentDocumentASparkFile()
                ? sparkSyntax.IsSparkSyntax(inputCharacter, out syntaxType)
                : false;
        }

        private bool IsMovementOrDeletionHandled(uint key)
        {
            if (ShouldDismissCompletion(key))
                _sparkOnlySession.Dismiss();

            if (!key.IsADeletionCharacter()) return false;

            if (!IsSparkOnlySessionActive()) return true;

            _sparkOnlySession.Filter();
            return true;
        }

        private bool ShouldDismissCompletion(uint key)
        {
            return
                IsSparkOnlySessionActive() &&
                key.IsAMovementCharacter() &&
                key.HasMovedOutOfIntelliSenseRange(_textExplorer);
        }

        public bool StartCompletionSession(SparkSyntaxTypes syntaxType)
        {
            if (!_config.TryCreateCompletionSession(_textExplorer, out _sparkOnlySession)) return false;
            var viewExplorer = ViewExplorer.CreateFromActiveDocument(_projectExplorer);
            _config.AddCompletionSourceProperties(
                new Dictionary<object, object> 
                {
                    {typeof(SparkSyntaxTypes), syntaxType},
                    {typeof(IViewExplorer), viewExplorer},
                    {typeof(ITextExplorer), _textExplorer},
                    {typeof(ITrackingSpan), _textExplorer.GetTrackingSpan()} 
                });

            _sparkOnlySession.Dismissed += OnSessionDismissed;
            _sparkOnlySession.Committed += OnSessionCommitted;
            _sparkOnlySession.Start();
            return IsSparkOnlySessionActive(); //Rob G: Depending on the content type - the session can sometimes be dismissed automatically
        }

        private void OnSessionCommitted(object sender, EventArgs e)
        {
            var point = _sparkOnlySession.TextView.Caret.Position.BufferPosition;
            if (point.Position > 1 && (point - 1).GetChar() == '"')
                _sparkOnlySession.TextView.Caret.MoveToPreviousCaretPosition();
            _sparkOnlySession.Dismiss();
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            _sparkOnlySession.Dismissed -= OnSessionDismissed;
            _sparkOnlySession = null;
        }
    }
}
