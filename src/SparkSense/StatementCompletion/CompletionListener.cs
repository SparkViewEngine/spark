using System;
using EnvDTE;
using SparkSense.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Language.Intellisense;

namespace SparkSense.StatementCompletion
{
    [Export(typeof (ICompletionSourceProvider))]
    [ContentType("spark")]
    [ContentType("HTML")]
    [Name("Spark Tag Completion")]
    public class CompletionListener : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService TextNavigator { get; set; }

        [Import(typeof(SVsServiceProvider))]
        public IServiceProvider ServiceProvider;

        public DTE VsEnvironment { get; private set; }

        #region ICompletionSourceProvider Members

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            var projectExplorer =  GetFromVsEnvironment();
            var textNavigator = TextNavigator.GetTextStructureNavigator(textBuffer);
            
            return new CompletionSource(textBuffer, textNavigator, projectExplorer);
        }

        #endregion

        private IProjectExplorer GetFromVsEnvironment()
        {
            IProjectExplorer projectExplorer = null;
            try
            {
                VsEnvironment = (DTE)ServiceProvider.GetService(typeof(DTE));
                if (VsEnvironment != null)
                    projectExplorer = new ProjectExplorer(VsEnvironment);
            }
            catch (COMException ex)
            {
                //TODO: Log the COM Exception somewhere
                //Unable to attach to the current visual studio environment
                VsEnvironment = null;
            }

            return projectExplorer;
        }

    }
}