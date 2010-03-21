using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace SparkSense.StatementCompletion
{
    [Export(typeof (IVsTextViewCreationListener))]
    [Name("Spark Tag Completion Listener")]
    [ContentType("spark")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class SparkCompletionCommandListener : IVsTextViewCreationListener
    {
        [Import] internal IVsEditorAdaptersFactoryService AdaptersFactoryService;

        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }

        #region IVsTextViewCreationListener Members

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            if (AdaptersFactoryService == null) return;
            IWpfTextView textView = AdaptersFactoryService.GetWpfTextView(textViewAdapter);
            if (textView == null) return;

            Func<SparkCompletionCommand> createCommand = () => new SparkCompletionCommand(textViewAdapter, textView, CompletionBroker);
            textView.Properties.GetOrCreateSingletonProperty(createCommand);
        }

        #endregion
    }
}