using EnvDTE;
using System;

namespace SparkSense
{
    public class SparkFileAnalyzer
    {
        private string _activeDocumentPath;
        private DTE _projectEnvironment;
        public SparkFileAnalyzer(DTE projectEnvironment)
        {
            if (projectEnvironment == null)
                throw new ArgumentNullException("projectEnvironment", "SparkFileAnalyzer requires a hook into the current visual studio enviroment.");

            _projectEnvironment = projectEnvironment;
        }

        public string ActiveDocumentPath
        {
            get
            {
                if (string.IsNullOrEmpty(_activeDocumentPath))
                {
                    _activeDocumentPath = _projectEnvironment.ActiveDocument != null
                        ? _projectEnvironment.ActiveDocument.FullName 
                        : string.Empty;
                }
                return _activeDocumentPath;
            }
        }
        public bool ViewFolderExists()
        {
            int viewsLocationStart = ActiveDocumentPath.LastIndexOf("Views");
            return viewsLocationStart != -1;

            //var viewRoot = CurrentDocument.FullName.Substring(0, viewsLocationStart + 5);
            //var currentView = CurrentDocument.FullName.Replace(viewRoot, string.Empty).TrimStart('\\');

            //var syntaxProvider = new DefaultSyntaxProvider(new ParserSettings());
            //var viewLoader = new ViewLoader { ViewFolder = new FileSystemViewFolder(viewRoot), SyntaxProvider = syntaxProvider };
            //viewLoader.Load(currentView);
            //var partials = viewLoader.FindPartialFiles(currentView);
        }
        public bool IsCurrentDocumentASparkFile()
        {
            return ActiveDocumentPath.EndsWith(".spark");
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
