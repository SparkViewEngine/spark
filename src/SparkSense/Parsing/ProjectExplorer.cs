using System;
using System.Collections.Generic;
using EnvDTE;
using Spark.FileSystem;

namespace SparkSense.Parsing
{
    public class ProjectExplorer : IProjectExplorer
    {
        private DTE _projectEnvironment;
        private CachingViewFolder _projectViewFolder;

        public ProjectExplorer(DTE projectEnvironment)
        {
            if (projectEnvironment == null)
                throw new ArgumentNullException("projectEnvironment", "Project Explorer requires a hook into the current visual studio enviroment.");

            _projectEnvironment = projectEnvironment;
        }

        private CachingViewFolder ProjectViewFolder
        {
            get
            {
                if (_projectViewFolder == null)
                {
                    string activeDocumentPath;
                    if (!TryGetActiveDocumentPath(out activeDocumentPath))
                        return _projectViewFolder;

                    _projectViewFolder = new CachingViewFolder(GetViewRoot(activeDocumentPath));
                    BuildViewMapFromProjectEnvironment();
                }
                return _projectViewFolder;
            }
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

            return ProjectViewFolder;
        }

        public string GetCurrentViewPath()
        {
            string activeDocumentPath;
            if (!TryGetActiveDocumentPath(out activeDocumentPath)) return null;
            return activeDocumentPath.Replace(GetViewRoot(activeDocumentPath), string.Empty).TrimStart('\\');
        }

        public bool HasView(string viewPath)
        {
            return ProjectViewFolder.HasView(viewPath);
        }

        private bool TryGetActiveDocumentPath(out string activeDocumentPath)
        {
            activeDocumentPath = _projectEnvironment.ActiveDocument != null ? _projectEnvironment.ActiveDocument.FullName : string.Empty;
            return !String.IsNullOrEmpty(activeDocumentPath);
        }

        private void BuildViewMapFromProjectEnvironment()
        {
            var currentProject = _projectEnvironment.ActiveDocument.ProjectItem.ContainingProject;
            foreach (ProjectItem projectItem in currentProject.ProjectItems)
                ScanProjectItemForViews(projectItem);
        }

        private void ScanProjectItemForViews(ProjectItem projectItem)
        {
            if (projectItem.Name.EndsWith(".spark"))
            {
                string projectItemMap = GetProjectItemMap(projectItem);
                if (!string.IsNullOrEmpty(projectItemMap))
                    ProjectViewFolder.Add(projectItemMap);
            }

            if (projectItem.ProjectItems != null)
                foreach (ProjectItem child in projectItem.ProjectItems)
                    ScanProjectItemForViews(child);
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

        private static string GetViewRoot(string activeDocumentPath)
        {
            int viewsLocationStart = activeDocumentPath.LastIndexOf("Views");
            return viewsLocationStart != -1 ? activeDocumentPath.Substring(0, viewsLocationStart + 5) : null;
        }
    }
}
