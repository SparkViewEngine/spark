using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using SparkSense.Parsing;
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

        [Import(typeof(SVsServiceProvider))]
        public IServiceProvider ServiceProvider;

        [Import]
        public ICompletionBroker CompletionBroker;

        [Import]
        internal ITextStructureNavigatorSelectorService TextNavigator { get; set; }

        public IVsTextView TextViewAdapter { get; private set; }
        public IWpfTextView TextView { get; private set; }
        public DTE VsEnvironment { get; private set; }
        public IProjectExplorer ProjectExplorer { get; private set; }

        #region IVsTextViewCreationListener Members

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            TextViewAdapter = textViewAdapter;
            if (AdaptersFactoryService == null || ServiceProvider == null || !TryGetEnvironmentAndView()) return;

            Func<KeyPressInterceptor> interceptionCreator = () => new KeyPressInterceptor(this);

            TextView.Properties.GetOrCreateSingletonProperty(interceptionCreator);
        }
        
        private bool TryGetEnvironmentAndView()
        {
            TextView = AdaptersFactoryService.GetWpfTextView(TextViewAdapter);
            try
            {
                VsEnvironment = (DTE)ServiceProvider.GetService(typeof(DTE));
                if (VsEnvironment != null)
                    ProjectExplorer = new ProjectExplorer(VsEnvironment);
            }
            catch (COMException ex)
            {
                //TODO: Log the COM Exception somewhere
                //Unable to attach to the current visual studio environment
                VsEnvironment = null;
            }

            return TextView != null && VsEnvironment != null;
        }
        #endregion
    }
}