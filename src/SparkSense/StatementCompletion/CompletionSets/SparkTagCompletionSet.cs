using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Spark.Compiler.NodeVisitors;
using System;
using SparkSense.Parsing;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public class SparkTagCompletionSet : SparkCompletionSetFactory
    {
        private static List<Completion> _specialNodeCompletions;
        private List<Completion> _completionList;

        public override IList<Completion> Completions
        {
            get
            {
                if (_completionList != null) return _completionList;

                _completionList = new List<Completion>();
                _completionList.AddRange(GetSpecialNodes());
                _completionList.AddRange(GetRelatedPartials());

                return _completionList;
            }
        }

        private static List<Completion> GetSpecialNodes()
        {
            if (_specialNodeCompletions != null) return _specialNodeCompletions;

            var chunkBuilder = new ChunkBuilderVisitor(new VisitorContext());
            var specialNodes = chunkBuilder.SpecialNodeMap.Keys;
            _specialNodeCompletions = new List<Completion>();

            foreach (var nodeName in specialNodes)
                _specialNodeCompletions.Add(new Completion(nodeName, nodeName, String.Format("Spark '{0}' tag", nodeName), SparkTagIcon, null));

            return _specialNodeCompletions;
        }
        private IEnumerable<Completion> GetRelatedPartials()
        {
            var relatedPartials = new List<Completion>();
            foreach (var partial in _viewExplorer.GetRelatedPartials())
                relatedPartials.Add(new Completion(partial, partial, string.Format("Partial found: '{0}'", partial), SparkTagIcon, null));

            return relatedPartials;
        }

    }
}