using System;
using System.Collections.Generic;
using EnvDTE;

namespace SparkSense.Parsing
{
    public class ProjectExplorer : IProjectExplorer
    {
        private string _activeDocumentPath;
        private DTE _projectEnvironment;
        private List<string> _viewMap;

        public ProjectExplorer(DTE projectEnvironment)
        {
            if (projectEnvironment == null)
                throw new ArgumentNullException("projectEnvironment", "Project Explorer requires a hook into the current visual studio enviroment.");

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

        public List<string> ViewMap
        {
            get
            {
                if (_viewMap == null)
                    _viewMap = BuildViewMapFromProjectEnvironment();
                return _viewMap;
            }
        }

        private List<string> BuildViewMapFromProjectEnvironment()
        {
            var viewMap = new List<string>();
            Solution solution = (Solution)_projectEnvironment.Solution;

            foreach (Project project in solution.Projects)
                foreach (ProjectItem projectItem in project.ProjectItems)
                    ScanProjectItemForViews(projectItem, viewMap);

            return viewMap;
        }

        private static void ScanProjectItemForViews(ProjectItem projectItem, List<string> viewMap)
        {
            if (projectItem.Name.EndsWith(".spark"))
                viewMap.Add(GetProjectItemMap(projectItem));

            if (projectItem.ProjectItems != null)
                foreach (ProjectItem child in projectItem.ProjectItems)
                    ScanProjectItemForViews(child, viewMap);
        }

        private static string GetProjectItemMap(ProjectItem projectItem)
        {
            string fullPath = projectItem.Properties.Item("FullPath").Value.ToString();

            int viewsLocationStart = fullPath.LastIndexOf("Views");
            var viewRoot = fullPath.Substring(0, viewsLocationStart + 5);
            var foundView = fullPath.Replace(viewRoot, string.Empty).TrimStart('\\');

            return foundView;
        }

        public bool ViewFolderExists()
        {
            int viewsLocationStart = ActiveDocumentPath.LastIndexOf("Views");
            return viewsLocationStart != -1;
        }

        public bool IsCurrentDocumentASparkFile()
        {
            return ActiveDocumentPath.EndsWith(".spark");
        }
    }
}
