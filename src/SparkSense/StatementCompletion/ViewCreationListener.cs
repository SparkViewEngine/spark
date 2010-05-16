using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

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

        public IVsHierarchy VsHierarchy;

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

            var projectExplorer = new SparkSense.Parsing.ProjectExplorer(vsEnvironment);

            Func<KeyPressInterceptor> interceptionCreator = 
                () => new KeyPressInterceptor(textViewAdapter, textView, CompletionBroker, projectExplorer);

            textView.Properties.GetOrCreateSingletonProperty(interceptionCreator);
        }
        
        private bool TryGetEnvironmentAndView(IVsTextView textViewAdapter, out IWpfTextView textView, out DTE vsEnvironment)
        {
            textView = AdaptersFactoryService.GetWpfTextView(textViewAdapter);
            try
            {
                vsEnvironment = (DTE)ServiceProvider.GetService(typeof(DTE));
            }
            catch (COMException ex)
            {
                //TODO: Log the COM Exception
                //Unable to load the visual studio environment
                vsEnvironment = null;
            }

            return textView != null && vsEnvironment != null;
        }
        #endregion
    }
}