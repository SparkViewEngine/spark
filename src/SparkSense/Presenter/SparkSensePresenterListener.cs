using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace SparkSense.Presenter
{
    [Export(typeof(IIntellisensePresenterProvider))]
    [ContentType("Spark")]
    [ContentType("HTML")]
    [Order(Before="Default")]
    [Name("SparkSense Presenter")]
    public class SparkSensePresenterListener : IIntellisensePresenterProvider 
    {
        
        #region IIntellisensePresenterProvider Members

        public IIntellisensePresenter TryCreateIntellisensePresenter(IIntellisenseSession session)
        {
            ICompletionSession completionSession = session as ICompletionSession;
            if (completionSession == null) return null;

            return null; // return new SparkSensePresenter(completionSession);
        }

        #endregion
    }
}
