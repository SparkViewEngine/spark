using EnvDTE;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using SparkSense.Parsing;
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

namespace SparkSense.StatementCompletion
{
    [Export(typeof (ICompletionSourceProvider))]
    [ContentType("spark")]
    [ContentType("HTML")]
    [Name("Spark Tag Completion")]
    public class CompletionListener : ICompletionSourceProvider
    {
        [Import(typeof(SVsServiceProvider))]
        public IServiceProvider ServiceProvider;

        public DTE VsEnvironment { get; private set; }

        #region ICompletionSourceProvider Members

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            var projectExplorer =  GetFromVsEnvironment();
            return new CompletionSource(textBuffer, projectExplorer);
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