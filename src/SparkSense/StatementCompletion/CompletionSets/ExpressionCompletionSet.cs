using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using System;
using SparkSense.Parsing;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public class ExpressionCompletionSet : CompletionSetFactory
    {
        private List<Completion> _completionList;

        protected override IList<Completion> GetCompletionSetForNodeAndContext()
        {
            if (_completionList != null)
                return _completionList;
            _completionList = new List<Completion>();
            _completionList.AddRange(GetVariables());
            return _completionList;
        }

        private IEnumerable<Completion> GetVariables()
        {
            var variables = new List<Completion>();
            if (_viewExplorer == null) return variables;

            _viewExplorer.GetGlobalVariables().ToList().ForEach(
                variable => variables.Add(
                    new Completion(variable, variable, string.Format("Global Variable: '{0}'", variable), GetIcon(Constants.ICON_SparkGlobalVariable), null)));

            _viewExplorer.GetLocalVariables().ToList().ForEach(
                variable => variables.Add(
                    new Completion(variable, variable, string.Format("Local Variable: '{0}'", variable), GetIcon(Constants.ICON_SparkLocalVariable), null)));

            _viewExplorer.GetLocalMacros().ToList().ForEach(
                variable => variables.Add(
                    new Completion(variable, variable, string.Format("Local Macro: '{0}'", variable), GetIcon(Constants.ICON_SparkMacro), null)));

            return variables;
        }
    }
}