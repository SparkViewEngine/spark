using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace SparkSense.StatementCompletion
{
    internal class KeyPressInterceptor : IOleCommandTarget
    {
        private readonly IVsTextView _textViewAdapter;
        private readonly CompletionSessionManager _sessionManager;
        private IOleCommandTarget _nextCommand;

        public KeyPressInterceptor(ViewCreationListener createdView)
        {
            _textViewAdapter = createdView.TextViewAdapter;
            var textNavigator = createdView.TextNavigator.GetTextStructureNavigator(createdView.TextView.TextBuffer);
            _sessionManager = new CompletionSessionManager(createdView.CompletionBroker, createdView.TextView, textNavigator);

            TryChainTheNextCommand();
        }

        private void TryChainTheNextCommand()
        {
            if (_textViewAdapter != null) _textViewAdapter.AddCommandFilter(this, out _nextCommand);
        }

        #region IOleCommandTarget Members

        public int QueryStatus(ref Guid cmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommand.QueryStatus(ref cmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid cmdGroup, uint key, uint cmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            char inputCharacter = key.GetInputCharacter(cmdGroup, pvaIn);

            if (_sessionManager.IsCompletionCommitted(key, inputCharacter)) return VSConstants.S_OK;

            int keyPressResult = _nextCommand.Exec(ref cmdGroup, key, cmdExecOpt, pvaIn, pvaOut);
            return _sessionManager.IsCompletionStarted(key, inputCharacter) ? VSConstants.S_OK : keyPressResult;
        }

        #endregion
    }
}