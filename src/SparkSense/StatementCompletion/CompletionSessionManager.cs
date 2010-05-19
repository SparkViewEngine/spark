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
        private CompletionSessionConfiguration _config;
        private IProjectExplorer _projectExplorer;
        private IWpfTextView _textView;
        private int _startPosition;
        private ITrackingSpan _trackingSpan;
        private ICompletionSession _session;

        public CompletionSessionManager(ICompletionBroker completionBroker, IProjectExplorer projectExplorer, IWpfTextView textView)
        {
            _completionBroker = completionBroker;
            _projectExplorer = projectExplorer;
            _textView = textView;

            _config = new CompletionSessionConfiguration(_completionBroker);
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
                key.HasMovedOutOfIntelliSenseRange(_textView, _trackingSpan, _startPosition);
        }

        public bool StartCompletionSession(SparkSyntaxTypes syntaxType)
        {
            SnapshotPoint? caretPoint = _textView.Caret.Position.BufferPosition;
            if (!caretPoint.HasValue) return false;

            ITextExplorer textExplorer = GetTextExplorer(caretPoint.Value);
            if (!_config.TryCreateCompletionSession(textExplorer, out _session)) return false;

            var viewExplorer = ViewExplorer.CreateFromActiveDocument(_projectExplorer);
            _config.AddCompletionSourceProperties(new List<object> { syntaxType, viewExplorer, _trackingSpan });
            
            _session.Dismissed += OnSessionDismissed;
            _session.Committed += OnSessionCommitted;
            _session.Start();
            return IsSessionActive(); //Rob G: Depending on the content type - the session can sometimes be dismissed automatically
        }

        private ITextExplorer GetTextExplorer(SnapshotPoint caretPoint)
        {
            _startPosition = caretPoint.Position;
            _trackingSpan = caretPoint.Snapshot.CreateTrackingSpan(caretPoint.Position, 0, SpanTrackingMode.EdgeInclusive);
            var trackingPoint = caretPoint.Snapshot.CreateTrackingPoint(caretPoint.Position, PointTrackingMode.Positive);
            return new TextExplorer(_textView, trackingPoint);
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
