using System;
using System.Collections.Generic;
using EnvDTE;
using Spark.FileSystem;

namespace SparkSense.Parsing
{
    public class ProjectExplorer : IProjectExplorer
    {
        private DTE _projectEnvironment;
        private List<string> _viewMap;

        public ProjectExplorer(DTE projectEnvironment)
        {
            if (projectEnvironment == null)
                throw new ArgumentNullException("projectEnvironment", "Project Explorer requires a hook into the current visual studio enviroment.");

            _projectEnvironment = projectEnvironment;
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

        public bool TryGetActiveDocumentPath(out string activeDocumentPath)
        {
            activeDocumentPath = _projectEnvironment.ActiveDocument != null ? _projectEnvironment.ActiveDocument.FullName : string.Empty;
            return activeDocumentPath != string.Empty;
        }

        public bool ViewFolderExists()
        {
            string activeDocumentPath;
            if (!TryGetActiveDocumentPath(out activeDocumentPath)) return false;

            int viewsLocationStart = activeDocumentPath.LastIndexOf("Views");
            return viewsLocationStart != -1;
        }
     
        public IViewFolder GetViewFolder()
        {
            string activeDocumentPath;
            if (!TryGetActiveDocumentPath(out activeDocumentPath)) return null;
            return new FileSystemViewFolder(GetViewRoot());
        }

        public string GetCurrentView()
        {
            string activeDocumentPath;
            if (!TryGetActiveDocumentPath(out activeDocumentPath)) return null;
            return activeDocumentPath.Replace(GetViewRoot(), string.Empty).TrimStart('\\');
        }

        public bool IsCurrentDocumentASparkFile()
        {
            string activeDocumentPath;
            if (!TryGetActiveDocumentPath(out activeDocumentPath)) return false;
            return activeDocumentPath.EndsWith(".spark");
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
            {
                string projectItemMap = GetProjectItemMap(projectItem);
                if (!string.IsNullOrEmpty(projectItemMap))
                    viewMap.Add(projectItemMap);
            }

            if (projectItem.ProjectItems != null)
                foreach (ProjectItem child in projectItem.ProjectItems)
                    ScanProjectItemForViews(child, viewMap);
        }

        private static string GetProjectItemMap(ProjectItem projectItem)
        {
            if (projectItem.Properties == null) return null;

            string fullPath = projectItem.Properties.Item("FullPath").Value.ToString();

            int viewsLocationStart = fullPath.LastIndexOf("Views");
            var viewRoot = fullPath.Substring(0, viewsLocationStart + 5);
            var foundView = fullPath.Replace(viewRoot, string.Empty).TrimStart('\\');

            return foundView;
        }
        
        private string GetViewRoot()
        {
            string activeDocumentPath;
            if (!TryGetActiveDocumentPath(out activeDocumentPath)) return null;
            int viewsLocationStart = activeDocumentPath.LastIndexOf("Views");
            return viewsLocationStart != -1 ? activeDocumentPath.Substring(0, viewsLocationStart + 5) : null;
        }
    }
}
