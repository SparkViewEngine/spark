using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Spark.Parser.Markup;
using SparkSense.Parsing;
using System;
using System.Collections.Generic;

namespace SparkSense.StatementCompletion
{
    public class CompletionSessionManager
    {
        private ICompletionBroker _completionBroker;
        private IProjectExplorer _projectExplorer;
        private bool _scanForNewSession;
        private IWpfTextView _textView;
        private ICompletionSession _sparkOnlySession;
        private ITextStructureNavigator _textNavigator;
        private ITrackingSpan _trackingSpan;

        public CompletionSessionManager(ICompletionBroker broker, IProjectExplorer projectExplorer, IWpfTextView textView, ITextStructureNavigator textNavigator)
        {
            if (broker == null) throw new ArgumentNullException("broker", "Session Config is null.");
            if (projectExplorer == null) throw new ArgumentNullException("projectExplorer", "Project Explorer is null.");
            if (textView == null) throw new ArgumentNullException("textView", "Text View is null.");
            if (textNavigator == null) throw new ArgumentNullException("textNavigator", "textNavigator is null.");

            _completionBroker = broker;
            _projectExplorer = projectExplorer;
            _textView = textView;
            _textNavigator = textNavigator;
        }

        public bool IsCompletionSessionActive()
        {
            return _completionBroker.IsCompletionActive(_textView);
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
            var caretPoint = _textView.Caret.Position.BufferPosition;
            if (!_scanForNewSession && inputCharacter == Char.MinValue && !key.IsStartCompletionCharacter()) return false;
            if (IsCompletionSessionActive()) return true;

            if (!IsSparkSyntax(caretPoint.Position))
                return IsMovementOrDeletionHandled(key);

            if (IsSparkOnlySessionActive() || StartCompletionSession())
                _sparkOnlySession.Filter();
            _scanForNewSession = false;
            return true;
        }

        private bool IsSparkSyntax(int caretPosition)
        {
            if (!_projectExplorer.IsCurrentDocumentASparkFile()) return false;

            var currentContext = SparkSyntax.ParseContext(_textView.TextBuffer.CurrentSnapshot.GetText(), caretPosition);
            return currentContext != null && currentContext != typeof(TextNode);
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
                key.HasMovedOutOfIntelliSenseRange(_textView, _sparkOnlySession);
        }

        public bool StartCompletionSession()
        {
            if (!TryCreateCompletionSession()) return false;
            var viewExplorer = new ViewExplorer(_projectExplorer);
            AddCompletionSourceProperties(
                new Dictionary<object, object> 
                {
                    {typeof(IViewExplorer), viewExplorer},
                    {typeof(ITrackingSpan), _trackingSpan}
                });

            _sparkOnlySession.Dismissed += OnSessionDismissed;
            _sparkOnlySession.Committed += OnSessionCommitted;
            _sparkOnlySession.Start();
            return IsSparkOnlySessionActive();
        }

        public bool TryCreateCompletionSession()
        {
            var caret = _textView.Caret.Position.Point.GetPoint(
                textBuffer => (textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
            if (!caret.HasValue) return false;

            var trackingPoint = caret.Value.Snapshot.CreateTrackingPoint(caret.Value.Position, PointTrackingMode.Positive);
            _trackingSpan = caret.Value.Snapshot.CreateTrackingSpan(caret.Value.Position, 0, SpanTrackingMode.EdgeInclusive);

            _sparkOnlySession = _completionBroker.CreateCompletionSession(_textView, trackingPoint, true);
            return _sparkOnlySession != null;
        }

        public void AddCompletionSourceProperties(Dictionary<object, object> properties)
        {
            if (properties == null) return;
            foreach (var property in properties)
                _sparkOnlySession.Properties.AddProperty(property.Key, property.Value);
        }

        private bool IsCompletionChar(char completionChar)
        {
            var point = _sparkOnlySession.TextView.Caret.Position.BufferPosition;
            return point.Position > 1 && (point - 1).GetChar() == completionChar;
        }
        private void ScanForPossibleNewSession()
        {
            if (!IsSparkOnlySessionActive()) return;
            if (!IsCompletionChar(Constants.DOUBLE_QUOTE) 
                && !IsCompletionChar(Constants.SINGLE_QUOTE)) return;

            _scanForNewSession = true;
            _sparkOnlySession.TextView.Caret.MoveToPreviousCaretPosition();
        }
        private void OnSessionCommitted(object sender, EventArgs e)
        {
            ScanForPossibleNewSession();
            _sparkOnlySession.Dismiss();
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            _sparkOnlySession.Dismissed -= OnSessionDismissed;
            _sparkOnlySession = null;
        }

    }
}
