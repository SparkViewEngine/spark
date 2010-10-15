using System;
using EnvDTE;
using Spark.FileSystem;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Design;

namespace SparkSense.Parsing
{
    public class ProjectExplorer : IProjectExplorer
    {
        private CachingViewFolder _projectViewFolder;
        private ISparkServiceProvider _services;
        private IVsHierarchy _hier;
        private ITypeResolutionService _resolver;
        private ITypeDiscoveryService _discovery;

        public ProjectExplorer(ISparkServiceProvider services)
        {
            if (services == null) throw new ArgumentNullException("services", "services is null.");
            _services = services;
        }
        private CachingViewFolder ProjectViewFolder
        {
            get
            {
                if (_projectViewFolder == null)
                {
                    string activeDocumentPath;
                    if (!TryGetActiveDocumentPath(out activeDocumentPath)) return _projectViewFolder;

                    _projectViewFolder = new CachingViewFolder(GetViewRoot(activeDocumentPath) ?? string.Empty);
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

        public IViewExplorer GetViewExplorer(ITextBuffer textBuffer)
        {
            IViewExplorer viewExplorer;
            if (textBuffer.Properties.TryGetProperty(typeof(ViewExplorer), out viewExplorer)) 
                return viewExplorer;

            viewExplorer = new ViewExplorer(this, GetCurrentViewPath(textBuffer));
            textBuffer.Properties.AddProperty(typeof(ViewExplorer), viewExplorer);
            return viewExplorer;
        }

        public string GetCurrentViewPath(ITextBuffer textBuffer)
        {
            var adapter = _services.AdaptersFactoryService.GetBufferAdapter(textBuffer) as IPersistFileFormat;
            if (adapter == null) return string.Empty;

            string filename;
            uint format;
            adapter.GetCurFile(out filename, out format);
            return filename.Replace(GetViewRoot(filename), string.Empty).TrimStart('\\');
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

        public void SetViewContent(string viewPath, string content)
        {
            ProjectViewFolder.SetViewSource(viewPath, content);
        }

        private bool TryGetActiveDocumentPath(out string activeDocumentPath)
        {
            activeDocumentPath = _services.VsEnvironment.ActiveDocument != null
                ? _services.VsEnvironment.ActiveDocument.FullName
                : string.Empty;
            return !String.IsNullOrEmpty(activeDocumentPath);
        }

        private void BuildViewMapFromProjectEnvironment()
        {
            var currentProject = _services.VsEnvironment.ActiveDocument.ProjectItem.ContainingProject;
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

        private IVsHierarchy GetHierarchy()
        {
            if (_hier == null)
            {
                var sln = _services.GetService<IVsSolution>();
                string projectName = _services.VsEnvironment.ActiveDocument.ProjectItem.ContainingProject.UniqueName;
                sln.GetProjectOfUniqueName(projectName, out _hier);
            }
            return _hier;
        }

        public ITypeDiscoveryService GetTypeDiscoveryService()
        {
            if (_discovery == null)
                _discovery = SparkServiceProvider.TypeService.GetTypeDiscoveryService(GetHierarchy());

            return _discovery;
        }

        public ITypeResolutionService GetTypeResolverService()
        {
            if (_resolver == null)
                _resolver = SparkServiceProvider.TypeService.GetTypeResolutionService(GetHierarchy());

            return _resolver;
        }
    }
}
