using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.OLE.Interop;

namespace SparkSense.StatementCompletion
{
    internal class KeyPressInterceptor : IOleCommandTarget
    {
        private readonly ICompletionBroker _completionBroker;
        private readonly IWpfTextView _textView;
        private readonly IVsTextView _textViewAdapter;
        private int _completionCaretStartPosition;
        private ITrackingSpan _completionSpan;
        private IOleCommandTarget _nextCommand;
        private ICompletionSession _session;
        private SparkProjectExplorer _projectExplorer;

        public KeyPressInterceptor(IVsTextView textViewAdapter, IWpfTextView textView, ICompletionBroker completionBroker, SparkProjectExplorer projectExplorer)
        {
            _textViewAdapter = textViewAdapter;
            _textView = textView;
            _completionBroker = completionBroker;
            _projectExplorer = projectExplorer;
            TryChainTheNextCommand();
        }

        #region IOleCommandTarget Members

        public int QueryStatus(ref Guid cmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommand.QueryStatus(ref cmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid cmdGroup, uint key, uint cmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            char inputCharacter = key.GetInputCharacter(cmdGroup, pvaIn);

            if (CheckForCompletionCommit(key, inputCharacter)) return VSConstants.S_OK;

            int keyPressResult = _nextCommand.Exec(ref cmdGroup, key, cmdExecOpt, pvaIn, pvaOut);
            return CheckForCompletionStart(key, inputCharacter) ? VSConstants.S_OK : keyPressResult;
        }

        #endregion

        private bool CheckForCompletionStart(uint key, char inputCharacter)
        {
            SparkCompletionTypes completionType;

            if (!IsSparkSyntax(inputCharacter, out completionType))
                return IsMovementOrDeletionHandled(key);

            if (IsSessionActive() || StartCompletion(completionType))
                _session.Filter();
            return true;
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
                key.HasMovedOutOfIntelliSenseRange(_textView, _completionSpan, _completionCaretStartPosition);
        }

        private bool CheckForCompletionCommit(uint key, char inputCharacter)
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

        private bool IsSparkSyntax(char inputCharacter, out SparkCompletionTypes completionType)
        {
            completionType = SparkCompletionTypes.None;
            if (inputCharacter.Equals(char.MinValue)) return false;

            SnapshotPoint caretPoint;
            if (!TryGetCurrentCaretPoint(out caretPoint)) return false;

            if (!_projectExplorer.IsCurrentDocumentASparkFile()) return false;

            var sparkCompletionType = new CompletionTypeSelector(_projectExplorer, caretPoint.Snapshot.TextBuffer, caretPoint.Position);
            completionType = sparkCompletionType.GetCompletionType(inputCharacter);
            return SparkCompletionTypes.None != completionType;
        }

        private bool TryGetCurrentCaretPoint(out SnapshotPoint caretPoint)
        {
            caretPoint = new SnapshotPoint();
            SnapshotPoint? caret = _textView.Caret.Position.Point.GetPoint
                (textBuffer => _textView.TextBuffer == textBuffer, PositionAffinity.Predecessor);

            if (!caret.HasValue)
                return false;

            caretPoint = caret.Value;
            return true;
        }

        private void TryChainTheNextCommand()
        {
            if (_textViewAdapter != null) _textViewAdapter.AddCommandFilter(this, out _nextCommand);
        }

        private bool IsSessionActive()
        {
            return _session != null && !_session.IsDismissed;
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

        private bool StartCompletion(SparkCompletionTypes sparkCompletionType)
        {
            SnapshotPoint? currentPoint = _textView.Caret.Position.Point.GetPoint(match => !match.ContentType.IsOfType("projection"), PositionAffinity.Predecessor);
            if (!currentPoint.HasValue) return false;

            RecordCompletionStartingPoint(currentPoint.Value);
            ConfigureCompletionSession(currentPoint.Value, sparkCompletionType);
            return IsSessionActive(); //Rob G: Depending on the content type - the session can sometimes be dismissed automatically
        }

        private void RecordCompletionStartingPoint(SnapshotPoint currentPoint)
        {
            _completionCaretStartPosition = currentPoint.Position;
            _completionSpan = currentPoint.Snapshot.CreateTrackingSpan(currentPoint.Position, 0, SpanTrackingMode.EdgeInclusive);
        }

        private void ConfigureCompletionSession(SnapshotPoint currentPoint, SparkCompletionTypes sparkCompletionType)
        {
            ITrackingPoint trackingPoint = currentPoint.Snapshot.CreateTrackingPoint(currentPoint.Position, PointTrackingMode.Positive);
            _session = _completionBroker.CreateCompletionSession(_textView, trackingPoint, true);
            _session.Properties.AddProperty(typeof(SparkCompletionTypes), sparkCompletionType);
            _session.Properties.AddProperty(typeof(ITrackingSpan), _completionSpan);
            _session.Dismissed += OnSessionDismissed;
            _session.Committed += OnSessionCommitted;
            _session.Start();
        }
    }
}