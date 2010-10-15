using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using System;
using SparkSense.Parsing;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public enum ExpressionContexts { None, New, Dig }


    public class ExpressionCompletionSet : CompletionSetFactory
    {
        private List<Completion> _completionList;

        public ExpressionContexts ExpressionContext
        {
            get
            {
                if (!SparkSyntax.IsExpression(CurrentContent, _triggerPoint))
                    return ExpressionContexts.None;

                if (CurrentContent[_triggerPoint - 1] == Constants.PERIOD)
                    return ExpressionContexts.Dig;
                return ExpressionContexts.New;

            }
        }

        protected override IList<Completion> GetCompletionSetForNodeAndContext()
        {
            if (_completionList != null) return _completionList;
            _completionList = new List<Completion>();

            switch (ExpressionContext)
            {
                case ExpressionContexts.New:
                    _completionList.AddRange(GetVariables());
                    _completionList.AddRange(GetMembers());
                    break;
                case ExpressionContexts.Dig:
                    _completionList.AddRange(GetMembers());
                    break;
                case ExpressionContexts.None:
                default:
                    break;
            }
            return _completionList.SortAlphabetically();
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
        private IEnumerable<Completion> GetMembers()
        {
            var members = new List<Completion>();
            if (_viewExplorer == null) return members;

            switch (ExpressionContext)
            {
                case ExpressionContexts.New:
                    _viewExplorer.GetInitialTypes().ToList().ForEach(
                        member => members.Add(
                            new Completion(member, member, string.Format("Member: '{0}'", member), GetIcon(Constants.ICON_SparkMacroParameter), null)));
                    break;
                case ExpressionContexts.Dig:
                    _viewExplorer.GetMembers().ToList().ForEach(
                        member => members.Add(
                            new Completion(member, member, string.Format("Member: '{0}'", member), GetIcon(Constants.ICON_SparkMacroParameter), null)));
                    break;
            }
            return members;
        }
    }
}