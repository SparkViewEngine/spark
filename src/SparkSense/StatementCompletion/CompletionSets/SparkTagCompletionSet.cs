using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Spark.Compiler.NodeVisitors;
using System;

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

                return _completionList;
            }
        }

        //private void ListProjectItems(ProjectItems projectItems, int level)
        //{
        //    foreach (ProjectItem item in projectItems)
        //    {
        //        _projectItems.Add(string.Format("{0}:{1}", item.Name, level));
        //        ProjectItems childItems = item.ProjectItems as ProjectItems;
        //        if (childItems == null) continue;
        //        ListProjectItems(childItems, level + 1);
        //    }
        //}
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

    }
}