using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Spark.Compiler.NodeVisitors;
using SparkSense.Parsing;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public class ElementCompletionSet : CompletionSetFactory
    {
        private static List<Completion> _specialNodeCompletions;
        private List<Completion> _completionList;

        protected override IList<Completion> GetCompletionSetForNodeAndContext()
        {
            if (_completionList != null)
                return _completionList;
            _completionList = new List<Completion>();
            _completionList.AddRange(GetSpecialNodes());
            _completionList.AddRange(GetRelatedPartials());
            return _completionList.SortAlphabetically();
        }

        private List<Completion> GetSpecialNodes()
        {
            if (_specialNodeCompletions != null) return _specialNodeCompletions;

            var chunkBuilder = new ChunkBuilderVisitor(new VisitorContext());
            var specialNodes = chunkBuilder.SpecialNodeMap.Keys;
            _specialNodeCompletions = new List<Completion>();

            foreach (var nodeName in specialNodes)
                _specialNodeCompletions.Add(new Completion(nodeName, nodeName, String.Format("Spark element: '{0}'", nodeName), GetIcon(Constants.ICON_SparkElement), null));

            return _specialNodeCompletions;
        }

        private IEnumerable<Completion> GetRelatedPartials()
        {
            var relatedPartials = new List<Completion>();
            if (_viewExplorer != null)
                foreach (var partialName in _viewExplorer.GetRelatedPartials())
                    relatedPartials.Add(new Completion(partialName, partialName, string.Format("Related Partial: '{0}'", partialName), GetIcon(Constants.ICON_SparkPartial), null));

            return relatedPartials;
        }

    }
}