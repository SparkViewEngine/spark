using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using SparkSense.Parsing;
using Microsoft.VisualStudio.Text.Operations;

namespace SparkSense.StatementCompletion
{
    public class CompletionSessionManager
    {
        private ICompletionSessionConfiguration _config;
        private IProjectExplorer _projectExplorer;
        private IWpfTextView _textView;
        private ICompletionSession _session;
        private IViewExplorer _viewExplorer;
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
        }

        public bool IsSessionActive()
        {
            return _session != null && !_session.IsDismissed;
        }

        public bool CompletionCommitted(uint key, char inputCharacter)
        {
            if (!(key.IsACommitCharacter(inputCharacter) && IsSessionActive()))
                return false;

            if (_session.SelectedCompletionSet.SelectionStatus.IsSelected)
            {
                _session.Commit();
                return true;
            }
            _session.Dismiss();
            return false;
        }

        public bool CompletionStarted(uint key, char inputCharacter)
        {
            SparkSyntaxTypes syntaxType;
            if (!TryEvaluateSparkSyntax(inputCharacter, out syntaxType))
                return IsMovementOrDeletionHandled(key);

            if (IsSessionActive() || StartCompletionSession(syntaxType))
                _session.Filter();
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
                _session.Dismiss();

            if (!key.IsADeletionCharacter()) return false;

            if (!IsSessionActive()) return true;

            _session.Filter();
            return true;
        }

        private bool ShouldDismissCompletion(uint key)
        {
            return
                IsSessionActive() &&
                key.IsAMovementCharacter() &&
                key.HasMovedOutOfIntelliSenseRange(_textExplorer);
        }

        public bool StartCompletionSession(SparkSyntaxTypes syntaxType)
        {
            _viewExplorer = ViewExplorer.CreateFromActiveDocument(_projectExplorer);
            _textExplorer = new TextExplorer(_textView, _textNavigator);

            if (!_config.TryCreateCompletionSession(_textExplorer, out _session)) return false;
            _config.AddCompletionSourceProperties(new List<object> { syntaxType, _viewExplorer, _textExplorer, _textExplorer.GetTrackingSpan() });
            
            _session.Dismissed += OnSessionDismissed;
            _session.Committed += OnSessionCommitted;
            _session.Start();
            return IsSessionActive(); //Rob G: Depending on the content type - the session can sometimes be dismissed automatically
        }

        private void OnSessionCommitted(object sender, EventArgs e)
        {
            var point = _session.TextView.Caret.Position.BufferPosition;
            if (point.Position > 1 && (point - 1).GetChar() == '"')
                _session.TextView.Caret.MoveToPreviousCaretPosition();
            _session.Dismiss();
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            _session.Dismissed -= OnSessionDismissed;
            _session = null;
        }
    }
}
