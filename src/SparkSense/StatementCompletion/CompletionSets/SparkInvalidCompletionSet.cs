using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public class SparkInvalidCompletionSet : SparkCompletionSetFactory
    {
        private List<Completion> _completionList;
        public override IList<Completion> Completions
        {
            get
            {
                if (_completionList != null) return _completionList;

                _completionList = new List<Completion> { 
                                    new Completion("Invalid Config", string.Empty, "Unable to find the Views Folder in the project.", null, string.Empty)
                                };
                return _completionList;
            }
        }
    }
}
