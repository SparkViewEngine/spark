using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public class SparkVariableCompletionSet : SparkCompletionSetFactory
    {
        private List<Completion> _completionList;

        public override IList<Completion> Completions
        {
            get
            {
                if (_completionList != null) return _completionList;

                _completionList = new List<Completion>
                                      {
                                          new Completion("some", "some", "Spark 'some' variable", SparkTagIcon, null),
                                          new Completion("variable", "variable", "Spark 'variable' variable", SparkTagIcon, null),
                                      };
                return _completionList;
            }
        }
    }
}