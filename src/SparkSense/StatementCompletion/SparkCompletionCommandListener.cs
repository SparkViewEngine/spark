using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using System.Collections.Generic;

namespace SparkSense.StatementCompletion
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Spark Tag Completion Listener")]
    [ContentType("spark")]
    [ContentType("HTML")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class SparkCompletionCommandListener : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdaptersFactoryService;

        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider ServiceProvider { get; set; }

        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }

        #region IVsTextViewCreationListener Members

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            if (AdaptersFactoryService == null || ServiceProvider == null) return;

            IWpfTextView textView = AdaptersFactoryService.GetWpfTextView(textViewAdapter);
            if (textView == null) return;

            var vsEnvironment = (DTE)ServiceProvider.GetService(typeof(DTE));
            SparkFileAnalyzer sparkFileAnalyzer = new SparkFileAnalyzer(vsEnvironment);

            Func<SparkCompletionCommand> createCommand = () => new SparkCompletionCommand(textViewAdapter, textView, CompletionBroker, sparkFileAnalyzer);
            textView.Properties.GetOrCreateSingletonProperty(createCommand);
        }
        #endregion
    }
}