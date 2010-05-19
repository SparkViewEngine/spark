using System;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using SparkSense.Parsing;
using System.Collections.Generic;

namespace SparkSense.StatementCompletion
{
    public class CompletionSessionManager
    {
        private ICompletionBroker _completionBroker;
        private IProjectExplorer _projectExplorer;
        private IWpfTextView _textView;
        private int _completionCaretStartPosition;
        private ITrackingSpan _completionSpan;
        private ICompletionSession _session;

        public CompletionSessionManager(ICompletionBroker completionBroker, IProjectExplorer projectExplorer, IWpfTextView textView)
        {
            _completionBroker = completionBroker;
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
            var sparkSyntax = new SparkSyntax(_projectExplorer, _textView);
            SparkSyntaxTypes syntaxType;

            if (!sparkSyntax.IsSparkSyntax(inputCharacter, out syntaxType))
                return IsMovementOrDeletionHandled(key);

            if (IsSessionActive() || StartCompletionSession(syntaxType))
                _session.Filter();
            return true;
        }

        public bool IsMovementOrDeletionHandled(uint key)
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
                key.HasMovedOutOfIntelliSenseRange(_textView, _completionSpan, _completionCaretStartPosition);
        }

        public bool StartCompletionSession(SparkSyntaxTypes syntaxType)
        {
            SnapshotPoint? caretPoint = _textView.Caret.Position.BufferPosition;
            if (!caretPoint.HasValue) return false;

            if (!ConfigureCompletionSession(caretPoint.Value, syntaxType)) return false;

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

        private bool ConfigureCompletionSession(SnapshotPoint currentPoint, SparkSyntaxTypes sparksyntaxType)
        {
            _completionCaretStartPosition = currentPoint.Position;
            _completionSpan = currentPoint.Snapshot.CreateTrackingSpan(currentPoint.Position, 0, SpanTrackingMode.EdgeInclusive);

            IViewExplorer viewExplorer = ViewExplorer.CreateFromActiveDocumentPath(_projectExplorer.ActiveDocumentPath);
            ITrackingPoint trackingPoint = currentPoint.Snapshot.CreateTrackingPoint(currentPoint.Position, PointTrackingMode.Positive);

            var sessionConfiguration = new CompletionSessionConfiguration(_completionBroker);

            //TODO: change this to use the config class instead
            _session =   _completionBroker.CreateCompletionSession(_textView, trackingPoint, true);
            if (_session == null) return false;

            sessionConfiguration.AddCompletionSourceProperties(new List<object> { sparksyntaxType, viewExplorer, _completionSpan });

            return true;
        }
    }
}
