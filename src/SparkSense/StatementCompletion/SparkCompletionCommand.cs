using System;
using System.Runtime.InteropServices;
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
        private readonly IOleCommandTarget _nextCommand;
        private readonly IWpfTextView _textView;
        private ICompletionSession _session;

        public SparkCompletionCommand(IVsTextView textViewAdapter, IWpfTextView textView, ICompletionBroker completionBroker)
        {
            _textView = textView;
            _completionBroker = completionBroker;
            textViewAdapter.AddCommandFilter(this, out _nextCommand);
        }

        #region IOleCommandTarget Members

        public int QueryStatus(ref Guid cmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommand.QueryStatus(ref cmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid cmdGroup, uint cmdId, uint cmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            uint commandId = cmdId;
            char inputCharacter = char.MinValue;

            if (cmdGroup == VSConstants.VSStd2K && cmdId == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                inputCharacter = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            if (IsACommitCharacter(cmdId, inputCharacter))
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

            int result = _nextCommand.Exec(ref cmdGroup, cmdId, cmdExecOpt, pvaIn, pvaOut);
            bool handled = false;

            if (!inputCharacter.Equals(char.MinValue) && char.IsLetterOrDigit(inputCharacter))
            {
                if (_session == null || _session.IsDismissed)
                {
                    if (StartCompletion() && _session != null)
                        _session.Filter();
                }
                else
                    _session.Filter();
                handled = true;
            }
            else if (IsADeletionCharacter(commandId))
            {
                if (_session != null && !_session.IsDismissed)
                    _session.Filter();
                handled = true;
            }

            return handled ? VSConstants.S_OK : result;
        }

        #endregion

        private bool StartCompletion()
        {
            SnapshotPoint? currentPoint = _textView.Caret.Position.Point.GetPoint(match => !match.ContentType.IsOfType("projection"), PositionAffinity.Predecessor);
            if (!currentPoint.HasValue) return false;

            ITrackingPoint trackingPoint = currentPoint.Value.Snapshot.CreateTrackingPoint(currentPoint.Value.Position, PointTrackingMode.Positive);
            _session = _completionBroker.CreateCompletionSession(_textView, trackingPoint, true);
            _session.Dismissed += OnSessionDismissed;
            _session.Start();
            return true;
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            _session.Dismissed -= OnSessionDismissed;
            _session = null;
        }

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