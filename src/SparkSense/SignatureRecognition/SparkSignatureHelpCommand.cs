using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;

namespace SparkSense.SignatureRecognition
{
    internal class SparkSignatureHelpCommand : IOleCommandTarget
    {
        private readonly ISignatureHelpBroker _broker;
        private readonly ITextStructureNavigator _navigator;
        private readonly IWpfTextView _textView;
        private IOleCommandTarget _nextCommand;
        private ISignatureHelpSession _session;

        public SparkSignatureHelpCommand(IVsTextView textViewAdapter, IWpfTextView textView, ITextStructureNavigator navigator, ISignatureHelpBroker broker)
        {
            _textView = textView;
            _navigator = navigator;
            _broker = broker;

            ChainTheNextCommand(textViewAdapter);
        }

        #region IOleCommandTarget Members

        public int QueryStatus(ref Guid cmdGroup, uint cmds, OLECMD[] prgCmds, IntPtr cmdText)
        {
            return _nextCommand.QueryStatus(cmdGroup, cmds, prgCmds, cmdText);
        }

        public int Exec(ref Guid cmdGroup, uint cmdId, uint cmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            char inputCharacter = char.MinValue;

            if (cmdGroup == VSConstants.VSStd2K && cmdId == (uint) VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                inputCharacter = (char) (ushort) Marshal.GetObjectForNativeVariant(pvaIn);
                if (inputCharacter.Equals(' '))
                {
                    SnapshotPoint currentPosition = _textView.Caret.Position.BufferPosition - 1;
                    TextExtent extentOfWord = _navigator.GetExtentOfWord(currentPosition);
                    string tagName = extentOfWord.Span.GetText();
                    _session = _broker.TriggerSignatureHelp(_textView);
                }
                else if (inputCharacter.Equals('>') && _session != null)
                {
                    _session.Dismiss();
                    _session = null;
                }
            }
            return _nextCommand.Exec(cmdGroup, cmdId, cmdExecOpt, pvaIn, pvaOut);
        }

        #endregion

        private void ChainTheNextCommand(IVsTextView textViewAdapter)
        {
            textViewAdapter.AddCommandFilter(this, out _nextCommand);
        }
    }
}