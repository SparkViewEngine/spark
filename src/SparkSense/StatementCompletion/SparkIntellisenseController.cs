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
    internal class SparkIntellisenseController : IIntellisenseController, IOleCommandTarget
    {
        private readonly SparkIntellisenseControllerProvider _provider;
        private readonly ITextBuffer _textBuffer;
        private readonly ITextView _textView;
        private readonly IWpfTextView _wpfTextView;
        private ITrackingSpan _completionSpan;
        private IOleCommandTarget _nextCommand;
        private ICompletionSession _session;
        private int _triggerPosition;

        public SparkIntellisenseController(SparkIntellisenseControllerProvider controllerProvider, ITextBuffer textBuffer, ITextView textView)
        {
            _provider = controllerProvider;
            _textBuffer = textBuffer;
            _textView = textView;
            _wpfTextView = _textView as IWpfTextView;

            SubscribeToKeyEvents();
            ChainTheNextCommand();
        }

        #region IIntellisenseController Members

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        public void Detach(ITextView textView)
        {
            //DetachKeyboardFilter();
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            if (_wpfTextView != null)
            {
                _wpfTextView.VisualElement.KeyDown -= VisualElement_KeyDown;
                _wpfTextView.VisualElement.KeyUp -= VisualElement_KeyUp;
                //DetachKeyboardFilter();
            }
        }

        #endregion

        #region IOleCommandTarget Members

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommand.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            //uint commandId = nCmdID;
            char inputCharacter = char.MinValue;

            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdId == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                inputCharacter = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            if (IsACommitCharacter(nCmdId, inputCharacter))
            {
                if (_session != null && !_session.IsDismissed)
                {
                    if (_session.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        _session.Commit();
                        return VSConstants.S_OK;
                    }
                    _session.Dismiss();
                }
            }

            return _nextCommand.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
            //int result = _nextCommand.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
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
            if (_wpfTextView == null) return;
            _wpfTextView.VisualElement.KeyDown += VisualElement_KeyDown;
            _wpfTextView.VisualElement.KeyUp += VisualElement_KeyUp;
        }

        private void ChainTheNextCommand()
        {
            IVsTextView textViewAdapter = _provider.AdaptersFactoryService.GetViewAdapter(_textView);
            if (textViewAdapter != null) textViewAdapter.AddCommandFilter(this, out _nextCommand);
        }

        private void VisualElement_KeyUp(object sender, KeyEventArgs e)
        {
            // Make sure that this event happened on the same text view to which we're attached.
            var textView = sender as ITextView;
            if (_textView != textView || textView == null)
                return;

            if (_session == null)
                return;

            switch (e.Key)
            {
                case Key.Space:
                case Key.Escape:
                    _session.Dismiss();
                    e.Handled = true;
                    return;

                case Key.Left:
                    if (textView.Caret.Position.BufferPosition.Position <= _triggerPosition)
                        // we went too far to the left
                        _session.Dismiss();
                    return;

                case Key.Right:
                    if (textView.Caret.Position.BufferPosition.Position >
                        _triggerPosition + _completionSpan.GetSpan(_completionSpan.TextBuffer.CurrentSnapshot).Length)
                        // we went too far to the right
                        _session.Dismiss();
                    return;

                case Key.Enter:
                    if (_session.SelectedCompletionSet.SelectionStatus != null)
                        _session.Commit();
                    else
                        _session.Dismiss();
                    e.Handled = true;
                    return;

                default:
                    break;
            }
        }

        private void VisualElement_KeyDown(object sender, KeyEventArgs e)
        {
            // Make sure that this event happened on the same text view to which we're attached.
            var textView = sender as ITextView;
            if (_textView != textView || textView == null)
                return;

            // if there is a session already leave it be
            if (_session != null)
                return;

            // determine which subject buffer is affected by looking at the caret position
            SnapshotPoint? caret = textView.Caret.Position.Point.GetPoint
                (textBuffer => _textBuffer == textBuffer, PositionAffinity.Predecessor);

            // return if no suitable buffer found
            if (!caret.HasValue)
                return;

            SnapshotPoint caretPoint = caret.Value;

            ITextBuffer subjectBuffer = caretPoint.Snapshot.TextBuffer;

            SparkCompletionTypes completionContext =
                SparkCompletionType.GetCompletionType(e.Key, subjectBuffer, caretPoint.Position);
            if (completionContext == SparkCompletionTypes.None)
                return;

            // the invocation occurred in a subject buffer of interest to us
            _triggerPosition = caretPoint.Position;
            ITrackingPoint triggerPoint = caretPoint.Snapshot.CreateTrackingPoint(_triggerPosition, PointTrackingMode.Negative);
            _completionSpan = caretPoint.Snapshot.CreateTrackingSpan(caretPoint.Position, 0, SpanTrackingMode.EdgeInclusive);

            // attach filter to intercept the Enter key
            //AttachKeyboardFilter();

            // Create a completion session
            _session = _provider.CompletionBroker.CreateCompletionSession(textView, triggerPoint, true);

            // Put the completion context and original (empty) completion span
            // on the session so that it can be used by the completion source
            _session.Properties.AddProperty(typeof(SparkCompletionTypes), completionContext);
            _session.Properties.AddProperty(typeof(SparkIntellisenseController), _completionSpan);

            // Attach to the session events
            _session.Dismissed += OnActiveSessionDismissed;
            _session.Committed += OnActiveSessionCommitted;

            // Start the completion session. The intellisense will be triggered.
            _session.Start();
        }

        private void OnActiveSessionDismissed(object sender, EventArgs e)
        {
            //DetachKeyboardFilter();
            _session = null;
        }

        private void OnActiveSessionCommitted(object sender, EventArgs e)
        {
            //DetachKeyboardFilter();
            //SnapshotPoint pos = _session.TextView.Caret.Position.BufferPosition;
            //if (pos.Position > 1)
            //    if ((pos - 1).GetChar() == '}' && ((pos - 2).GetChar() == '}' || (pos - 2).GetChar() == '%'))
            //    {
            //        ITextView textView = _session.TextView;
            //        textView.Caret.MoveToPreviousCaretPosition();
            //        textView.Caret.MoveToPreviousCaretPosition();
            //        textView.Caret.MoveToPreviousCaretPosition();
            //    }
            _session = null;
        }

        //private void AttachKeyboardFilter()
        //{
        //    ErrorHandler.ThrowOnFailure(_provider.AdaptersFactoryService.GetViewAdapter(_textView).AddCommandFilter(this, out _nextCommand));
        //}

        //private void DetachKeyboardFilter()
        //{
        //    ErrorHandler.ThrowOnFailure(_provider.AdaptersFactoryService.GetViewAdapter(_textView).RemoveCommandFilter(this));
        //}

        private static bool IsADeletionCharacter(uint commandId)
        {
            return commandId == (uint)VSConstants.VSStd2KCmdID.BACKSPACE ||
                   commandId == (uint)VSConstants.VSStd2KCmdID.DELETE;
        }

        private static bool IsACommitCharacter(uint cmdId, char inputCharacter)
        {
            return cmdId == (uint)VSConstants.VSStd2KCmdID.RETURN ||
                   cmdId == (uint)VSConstants.VSStd2KCmdID.TAB ||
                   char.IsWhiteSpace(inputCharacter) ||
                   char.IsPunctuation(inputCharacter);
        }
    }
}