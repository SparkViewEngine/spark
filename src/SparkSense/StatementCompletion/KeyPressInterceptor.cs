using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace SparkSense.StatementCompletion
{
    internal class KeyPressInterceptor : IOleCommandTarget
    {
        private ViewCreationListener _createdView;
        private readonly IVsTextView _textViewAdapter;
        private IOleCommandTarget _nextCommand;
        private CompletionSessionManager _sessionManager;

        public KeyPressInterceptor(ViewCreationListener createdView)
        {
            _createdView = createdView;
            _textViewAdapter = createdView.TextViewAdapter;
            _sessionManager = new CompletionSessionManager(new CompletionSessionConfiguration(_createdView.CompletionBroker), _createdView.ProjectExplorer, _createdView.TextView);

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

            if (_sessionManager.CompletionCommitted(key, inputCharacter)) return VSConstants.S_OK;

            int keyPressResult = _nextCommand.Exec(ref cmdGroup, key, cmdExecOpt, pvaIn, pvaOut);
            return _sessionManager.CompletionStarted(key, inputCharacter) ? VSConstants.S_OK : keyPressResult;
        }

        #endregion
    }
}