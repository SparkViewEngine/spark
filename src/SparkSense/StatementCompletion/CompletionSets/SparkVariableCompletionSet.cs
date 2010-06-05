using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using System;

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

                _completionList = new List<Completion>();
                _completionList.AddRange(GetVariables());

                return _completionList;
            }
        }

        private static IEnumerable<Completion> GetVariables()
        {
            var variables = new List<Completion>();
            if (_viewExplorer == null) return variables;

            _viewExplorer.GetGlobalVariables().ToList().ForEach(
                variable => variables.Add(
                    new Completion(variable, variable, string.Format("Global Variable found: '{0}'", variable), SparkTagIcon, null)));

            _viewExplorer.GetLocalVariables().ToList().ForEach(
                variable => variables.Add(
                    new Completion(variable, variable, string.Format("Local Variable found: '{0}'", variable), SparkTagIcon, null)));

            variables.Sort((c1, c2) => c1.DisplayText.CompareTo(c2.DisplayText));
            return variables;
        }
    }
}