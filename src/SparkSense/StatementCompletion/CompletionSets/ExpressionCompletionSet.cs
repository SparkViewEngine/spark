using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using System;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public class ExpressionCompletionSet : CompletionSetFactory
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

        private IEnumerable<Completion> GetVariables()
        {
            var variables = new List<Completion>();
            if (_viewExplorer == null) return variables;

            _viewExplorer.GetGlobalVariables().ToList().ForEach(
                variable => variables.Add(
                    new Completion(variable, variable, string.Format("Global Variable: '{0}'", variable), SparkGlobalVariableIcon, null)));

            _viewExplorer.GetLocalVariables().ToList().ForEach(
                variable => variables.Add(
                    new Completion(variable, variable, string.Format("Local Variable: '{0}'", variable), SparkLocalVariableIcon, null)));

            _viewExplorer.GetLocalMacros().ToList().ForEach(
                variable => variables.Add(
                    new Completion(variable, variable, string.Format("Local Macro: '{0}'", variable), SparkSparkMacroIcon, null)));

            return variables;
        }
    }
}