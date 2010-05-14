using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

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

        [Import(typeof(SVsServiceProvider))]
        public IServiceProvider ServiceProvider;

        [Import]
        public ICompletionBroker CompletionBroker;

        #region IVsTextViewCreationListener Members

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            DTE vsEnvironment;
            IWpfTextView textView;

            if (AdaptersFactoryService == null || ServiceProvider == null) return;
            if (!TryGetEnvironmentAndView(textViewAdapter, out textView, out vsEnvironment)) return;

            var projectExplorer = new SparkProjectExplorer(vsEnvironment);

            Func<KeyPressInterceptor> interceptionCreator = 
                () => new KeyPressInterceptor(textViewAdapter, textView, CompletionBroker, projectExplorer);

            textView.Properties.GetOrCreateSingletonProperty(interceptionCreator);
        }
        
        private bool TryGetEnvironmentAndView(IVsTextView textViewAdapter, out IWpfTextView textView, out DTE vsEnvironment)
        {
            textView = AdaptersFactoryService.GetWpfTextView(textViewAdapter);
            vsEnvironment = (DTE)ServiceProvider.GetService(typeof(DTE));

            return textView != null && vsEnvironment != null;
        }
        #endregion
    }
}