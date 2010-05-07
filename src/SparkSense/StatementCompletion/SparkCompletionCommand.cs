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
    internal class SparkCompletionCommand : IOleCommandTarget
    {
        private readonly ICompletionBroker _completionBroker;
        private readonly IWpfTextView _textView;
        private readonly IVsTextView _textViewAdapter;
        private int _completionCaretStartPosition;
        private ITrackingSpan _completionSpan;
        private IOleCommandTarget _nextCommand;
        private ICompletionSession _session;
        private SparkFileAnalyzer _sparkFileAnalyzer;

        public SparkCompletionCommand(IVsTextView textViewAdapter, IWpfTextView textView, ICompletionBroker completionBroker, SparkFileAnalyzer sparkFileAnalyzer)
        {
            _textViewAdapter = textViewAdapter;
            _textView = textView;
            _completionBroker = completionBroker;
            _sparkFileAnalyzer = sparkFileAnalyzer;
            TryChainTheNextCommand();
        }

        #region IOleCommandTarget Members

        public int QueryStatus(ref Guid cmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommand.QueryStatus(ref cmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid cmdGroup, uint key, uint cmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            char inputCharacter = GetInputCharacter(cmdGroup, key, pvaIn);
            if (IsACommitCharacter(key, inputCharacter))
            {
                if (IsSessionActive())
                {
                    if (_session.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        _session.Commit();
                        return VSConstants.S_OK;
                    }
                    _session.Dismiss();
                }
            }

            int result = _nextCommand.Exec(ref cmdGroup, key, cmdExecOpt, pvaIn, pvaOut);
            bool handled = false;
            SparkCompletionTypes completionType;

            if (IsSparkSyntax(inputCharacter, out completionType))
            {
                if (!IsSessionActive())
                {
                    if (StartCompletion(completionType))
                        _session.Filter();
                }
                else
                    _session.Filter();
                handled = true;
            }
            else if (IsADeletionCharacter(key))
            {
                if (IsSessionActive())
                    _session.Filter();
                handled = true;
            }
            else if (IsAMovementCharacter(key))
                if (IsSessionActive() && HasMovedOutOfIntellisenseRange(key))
                    _session.Dismiss();


            return handled ? VSConstants.S_OK : result;
        }

        #endregion

        private bool IsSparkSyntax(char inputCharacter, out SparkCompletionTypes completionType)
        {
            completionType = SparkCompletionTypes.None;
            if (inputCharacter.Equals(char.MinValue)) return false;

            SnapshotPoint caretPoint;
            if (!TryGetCurrentCaretPoint(out caretPoint)) return false;

            if (!_sparkFileAnalyzer.IsCurrentDocumentASparkFile()) return false;

            var sparkCompletionType = new SparkCompletionType(_sparkFileAnalyzer, caretPoint.Snapshot.TextBuffer, caretPoint.Position);
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

        private static bool IsACommitCharacter(uint key, char inputCharacter)
        {
            return key == (uint) VSConstants.VSStd2KCmdID.RETURN ||
                   key == (uint) VSConstants.VSStd2KCmdID.TAB ||
                   char.IsWhiteSpace(inputCharacter) ||
                   char.IsPunctuation(inputCharacter);
        }

        private static bool IsADeletionCharacter(uint key)
        {
            return key == (uint) VSConstants.VSStd2KCmdID.BACKSPACE ||
                   key == (uint) VSConstants.VSStd2KCmdID.DELETE;
        }

        private static bool IsAMovementCharacter(uint key)
        {
            return key == (uint) VSConstants.VSStd2KCmdID.LEFT ||
                   key == (uint) VSConstants.VSStd2KCmdID.RIGHT;
        }

        private bool HasMovedOutOfIntellisenseRange(uint key)
        {
            int currentPosition = _textView.Caret.Position.BufferPosition.Position;
            ITextSnapshot currentSnapshot = _completionSpan.TextBuffer.CurrentSnapshot;

            switch (key)
            {
                case (uint) VSConstants.VSStd2KCmdID.LEFT:
                    return currentPosition < _completionCaretStartPosition;
                case (uint) VSConstants.VSStd2KCmdID.RIGHT:
                    return currentPosition > _completionCaretStartPosition + _completionSpan.GetSpan(currentSnapshot).Length;
            }
            return false;
        }

        private static char GetInputCharacter(Guid cmdGroup, uint key, IntPtr pvaIn)
        {
            char inputCharacter = char.MinValue;

            if (cmdGroup == VSConstants.VSStd2K && key == (uint) VSConstants.VSStd2KCmdID.TYPECHAR)
                inputCharacter = (char) (ushort) Marshal.GetObjectForNativeVariant(pvaIn);
            return inputCharacter;
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
            _session.Properties.AddProperty(typeof (SparkCompletionTypes), sparkCompletionType);
            _session.Properties.AddProperty(typeof(ITrackingSpan), _completionSpan);
            _session.Dismissed += OnSessionDismissed;
            _session.Committed += OnSessionCommitted;
            _session.Start();
        }
    }
}