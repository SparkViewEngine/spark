using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace SparkSense.StatementCompletion
{
    internal class SparkCompletionCommand : IOleCommandTarget
    {
        private readonly ICompletionBroker _completionBroker;
        private readonly IWpfTextView _textView;
        private readonly IVsTextView _textViewAdapter;
        private ICompletionSession _activeSession;
        private ITrackingSpan _completionSpan;
        private IOleCommandTarget _nextCommand;
        private int _triggerPosition;

        public SparkCompletionCommand(IVsTextView textViewAdapter, IWpfTextView textView, ICompletionBroker completionBroker)
        {
            _textViewAdapter = textViewAdapter;
            _textView = textView;
            _completionBroker = completionBroker;
            SubscribeToKeyEvents();
        }

        #region IOleCommandTarget Members

        public int QueryStatus(ref Guid cmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommand.QueryStatus(ref cmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid cmdGroup, uint key, uint cmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            char inputCharacter = GetInputCharacter(cmdGroup, key, pvaIn);

            return IsACommitCharacter(key, inputCharacter)
                       ? VSConstants.S_OK
                       : _nextCommand.Exec(ref cmdGroup, key, cmdExecOpt, pvaIn, pvaOut);

            //if (IsSessionActive())
            //{
            //    if (IsACommitCharacter(key, inputCharacter))
            //    {
            //        if (_activeSession.SelectedCompletionSet.SelectionStatus.IsSelected)
            //            _activeSession.Commit();
            //        else
            //        _activeSession.Dismiss();
            //    }
            //    else if (IsADeletionCharacter(key))
            //        _activeSession.Filter();

            //    return VSConstants.S_OK;
            //}

            //int result = _nextCommand.Exec(ref cmdGroup, cmdId, cmdExecOpt, pvaIn, pvaOut);
            //bool handled = false;

            //if (!inputCharacter.Equals(char.MinValue) && inputCharacter.Equals('<'))
            //{
            //    if (_session == null || _session.IsDismissed)
            //    {
            //        if (StartCompletion() && _session != null)
            //            _session.Filter();
            //    }
            //    else
            //        _session.Filter();
            //    handled = true;
            //}
            //else if (IsADeletionCharacter(commandId))
            //{
            //    if (_session != null && !_session.IsDismissed)
            //        _session.Filter();
            //    handled = true;
            //}

            //return handled ? VSConstants.S_OK : result;
        }

        #endregion

        private void SubscribeToKeyEvents()
        {
            if (_textView == null) return;
            _textView.VisualElement.KeyUp += TextViewKeyUp;
            _textView.VisualElement.KeyDown += TextViewKeyDown;
        }

        private void ChainTheNextCommand()
        {
            if (_textViewAdapter != null) _textViewAdapter.AddCommandFilter(this, out _nextCommand);
        }

        private void RepositionCaretCorrectly()
        {
            //SnapshotPoint pos = _session.TextView.Caret.Position.BufferPosition;
            //if (pos.Position > 1)
            //    if ((pos - 1).GetChar() == '}' && ((pos - 2).GetChar() == '}' || (pos - 2).GetChar() == '%'))
            //    {
            //        ITextView textView = _session.TextView;
            //        textView.Caret.MoveToPreviousCaretPosition();
            //        textView.Caret.MoveToPreviousCaretPosition();
            //        textView.Caret.MoveToPreviousCaretPosition();
            //    }
        }

        private bool IsSessionActive()
        {
            return _activeSession != null && !_activeSession.IsDismissed;
        }

        private static bool IsACommitCharacter(uint key, char inputCharacter)
        {
            return key == (uint) VSConstants.VSStd2KCmdID.RETURN ||
                   key == (uint) VSConstants.VSStd2KCmdID.TAB;

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

        private static char GetInputCharacter(Guid cmdGroup, uint key, IntPtr pvaIn)
        {
            char inputCharacter = char.MinValue;

            if (cmdGroup == VSConstants.VSStd2K && key == (uint) VSConstants.VSStd2KCmdID.TYPECHAR)
                inputCharacter = (char) (ushort) Marshal.GetObjectForNativeVariant(pvaIn);
            return inputCharacter;
        }

        private void OnActiveSessionCommitted(object sender, EventArgs e)
        {
            RepositionCaretCorrectly();
            if (_textViewAdapter != null) _textViewAdapter.RemoveCommandFilter(this);
        }

        private void OnActiveSessionDismissed(object sender, EventArgs e)
        {
            if (_textViewAdapter != null) _textViewAdapter.RemoveCommandFilter(this);
            _activeSession.Dismissed -= OnActiveSessionDismissed;
            _activeSession = null;
        }

        private void TextViewKeyUp(object sender, KeyEventArgs e)
        {
            if (!IsCorrectTextView(sender)) return;
            if (!IsSessionActive()) return;

            switch (e.Key)
            {
                case Key.Escape:
                    _activeSession.Dismiss();
                    e.Handled = true;
                    return;

                case Key.Back:
                case Key.Delete:
                    _activeSession.Filter();
                    e.Handled = true;
                    return;

                case Key.Space:
                case Key.Tab:
                case Key.Enter:
                    if (_activeSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                        _activeSession.Commit();
                    else
                        _activeSession.Dismiss();
                    e.Handled = true;
                    return;

                case Key.Left:
                case Key.Right:
                    if (MovedOutOfIntellisenseRange(e.Key))
                        _activeSession.Dismiss();
                    return;
                default:
                    break;
            }
        }

        private void TextViewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsCorrectTextView(sender)) return;
            if (IsSessionActive()) return;

            // determine which subject buffer is affected by looking at the caret position
            SnapshotPoint? caret = _textView.Caret.Position.Point.GetPoint
                (textBuffer => _textView.TextBuffer == textBuffer, PositionAffinity.Predecessor);

            if (!caret.HasValue)
                return;

            SnapshotPoint caretPoint = caret.Value;

            ITextBuffer subjectBuffer = caretPoint.Snapshot.TextBuffer;

            SparkCompletionTypes completionType = SparkCompletionType.GetCompletionType(e.Key, subjectBuffer, caretPoint.Position);
            if (completionType == SparkCompletionTypes.None)
                return;

            ChainTheNextCommand();
            StartCompletion(caret, completionType);
            return;

            // the invocation occurred in a subject buffer of interest to us
            //_triggerPosition = caretPoint.Position;
            //ITrackingPoint triggerPoint = caretPoint.Snapshot.CreateTrackingPoint(_triggerPosition, PointTrackingMode.Negative);
            //_completionSpan = caretPoint.Snapshot.CreateTrackingSpan(caretPoint.Position, 0, SpanTrackingMode.EdgeInclusive);

            //// attach filter to intercept the Enter key

            //// Create a completion session
            //_activeSession = _completionBroker.CreateCompletionSession(_textView, triggerPoint, true);

            //// Put the completion context and original (empty) completion span
            //// on the session so that it can be used by the completion source
            //_activeSession.Properties.AddProperty(typeof (SparkCompletionTypes), completionType);
            //_activeSession.Properties.AddProperty(typeof (SparkIntellisenseController), _completionSpan);

            //// Attach to the session events
            //_activeSession.Dismissed += OnActiveSessionDismissed;
            //_activeSession.Committed += OnActiveSessionCommitted;

            //// Start the completion session. The intellisense will be triggered.
            //_activeSession.Start();
        }

        private bool IsCorrectTextView(object sender)
        {
            return sender != null 
                && sender as ITextView != null 
                && _textView == sender as ITextView;
        }


        private void StartCompletion(SnapshotPoint? currentPoint, SparkCompletionTypes sparkCompletionType)
        {
            //SnapshotPoint? currentPoint = _textView.Caret.Position.Point.GetPoint(match => !match.ContentType.IsOfType("projection"), PositionAffinity.Predecessor);
            if (!currentPoint.HasValue) return;

            ITrackingPoint trackingPoint = currentPoint.Value.Snapshot.CreateTrackingPoint(currentPoint.Value.Position, PointTrackingMode.Positive);
            _activeSession = _completionBroker.CreateCompletionSession(_textView, trackingPoint, true);
            _activeSession.Properties.AddProperty(typeof(SparkCompletionTypes), sparkCompletionType);
            _activeSession.Properties.AddProperty(typeof(SparkIntellisenseController), _completionSpan);
            _activeSession.Dismissed += OnActiveSessionDismissed;
            _activeSession.Committed += OnActiveSessionCommitted;
            _activeSession.Start();
            return;
        }

        private bool MovedOutOfIntellisenseRange(Key key)
        {
            var currentPosition = _textView.Caret.Position.BufferPosition.Position;
            return key == Key.Left
                       ? currentPosition <= _triggerPosition
                       : currentPosition > _triggerPosition + _completionSpan.GetSpan(_completionSpan.TextBuffer.CurrentSnapshot).Length;
        }
    }
}