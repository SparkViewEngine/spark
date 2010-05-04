using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public class SparkTagCompletionSet : SparkCompletionSetFactory
    {
        private List<Completion> _completionList;

        public override IList<Completion> Completions
        {
            get
            {
                if (_completionList != null) return _completionList;

                _completionList = new List<Completion>
                                      {
                                          new Completion("content", "<content", "Spark 'content' tag for spooling output to various text writers", SparkTagIcon, null),
                                          new Completion("default", "<default", "Spark 'default' tag for declaring local variables if a symbol of a given name is not known to be in scope",
                                                         SparkTagIcon, null),
                                      };
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


    }
}