using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace SparkSense.SignatureRecognition
{
    [Export(typeof (IVsTextViewCreationListener))]
    [Name("Spark Tag Signature Help Listener")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [ContentType("spark")]
    [ContentType("HTML")]
    internal class SparkSignatureHelpCommandListener : IVsTextViewCreationListener
    {
        [Import] internal IVsEditorAdaptersFactoryService AdapterService = null;
        [Import] internal ISignatureHelpBroker Broker = null;
        [Import] internal ITextStructureNavigatorSelectorService NavigatorService = null;

        #region IVsTextViewCreationListener Members

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null) return;
            ITextStructureNavigator navigator = NavigatorService.GetTextStructureNavigator(textView.TextBuffer);

            Func<SparkSignatureHelpCommand> createCommand = () => new SparkSignatureHelpCommand(textViewAdapter, textView, navigator, Broker);
            textView.Properties.GetOrCreateSingletonProperty(createCommand);
        }

        #endregion
    }
}