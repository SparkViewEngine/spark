using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using SparkSense.Parsing;

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

        public CompletionSessionManager(ICompletionSessionConfiguration config, IProjectExplorer projectExplorer, IWpfTextView textView)
        {
            if (config == null) throw new ArgumentNullException("config", "Session Config is null.");
            if (projectExplorer == null) throw new ArgumentNullException("projectExplorer", "Project Explorer is null.");
            if (textView == null) throw new ArgumentNullException("textView", "Text View is null.");

            _config = config;
            _projectExplorer = projectExplorer;
            _textView = textView;
        }

        public bool IsSessionActive()
        {
            return _session != null && !_session.IsDismissed;
        }

        public bool CheckForCompletionCommit(uint key, char inputCharacter)
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

        public bool CheckForCompletionStart(uint key, char inputCharacter)
        {
            var sparkSyntax = new SparkSyntax(_textExplorer);
            SparkSyntaxTypes syntaxType;

            if (!TryEvaluateSparkSyntax(inputCharacter, sparkSyntax, out syntaxType))
                return IsMovementOrDeletionHandled(key);

            if (IsSessionActive() || StartCompletionSession(syntaxType))
                _session.Filter();
            return true;
        }

        private bool TryEvaluateSparkSyntax(char inputCharacter, SparkSyntax sparkSyntax, out SparkSyntaxTypes syntaxType)
        {
            syntaxType = !_projectExplorer.ViewFolderExists() ? SparkSyntaxTypes.Invalid : SparkSyntaxTypes.None;

            return !_projectExplorer.IsCurrentDocumentASparkFile() 
                ? false 
                : sparkSyntax.IsSparkSyntax(inputCharacter, out syntaxType);
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
            _textExplorer = new TextExplorer(_textView);

            if (!_config.TryCreateCompletionSession(_textExplorer, out _session)) return false;
            _config.AddCompletionSourceProperties(new List<object> { syntaxType, _viewExplorer, _textExplorer.GetTrackingSpan() });
            
            _session.Dismissed += OnSessionDismissed;
            _session.Committed += OnSessionCommitted;
            _session.Start();
            return IsSessionActive(); //Rob G: Depending on the content type - the session can sometimes be dismissed automatically
        }

        private void OnSessionCommitted(object sender, EventArgs e)
        {
            //TODO: Rob G - Reposition Caret Correctly
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            _session.Dismissed -= OnSessionDismissed;
            _session = null;
        }
    }
}
