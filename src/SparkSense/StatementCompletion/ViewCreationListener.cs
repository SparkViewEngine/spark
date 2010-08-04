using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Operations;

namespace SparkSense.StatementCompletion
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Spark View Creation Listener")]
    [ContentType("spark")]
    [ContentType("HTML")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    public class ViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService AdaptersFactoryService;

        [Import]
        public ICompletionBroker CompletionBroker;

        [Import]
        public ITextStructureNavigatorSelectorService TextNavigator { get; set; }

        public IVsTextView TextViewAdapter { get; private set; }
        public IWpfTextView TextView { get; private set; }

        #region IVsTextViewCreationListener Members

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            TextViewAdapter = textViewAdapter;
            if (!TryGetTextView()) return;

            Func<KeyPressInterceptor> interceptionCreator = () => new KeyPressInterceptor(this);

            TextView.Properties.GetOrCreateSingletonProperty(interceptionCreator);
        }
        
        private bool TryGetTextView()
        {
            if (AdaptersFactoryService == null) return false;
            TextView = AdaptersFactoryService.GetWpfTextView(TextViewAdapter);
            return TextView != null;
        }
        #endregion
    }
}