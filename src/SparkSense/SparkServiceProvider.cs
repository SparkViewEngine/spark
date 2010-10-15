using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using SparkSense.Parsing;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Design;
using Microsoft.VisualStudio.Shell.Interop;

namespace SparkSense
{
    public interface ISparkServiceProvider
    {
        T GetService<T>();
        T GetService<T>(Type serviceType);
        IProjectExplorer ProjectExplorer { get; }
        DTE VsEnvironment { get; }
        IVsEditorAdaptersFactoryService AdaptersFactoryService { get; }
    }

    [Export(typeof(ISparkServiceProvider))]
    public class SparkServiceProvider : ISparkServiceProvider
    {
        public static readonly DynamicTypeService TypeService = (DynamicTypeService)Package.GetGlobalService(typeof(DynamicTypeService)); 

        [Import]
        private IVsEditorAdaptersFactoryService _adaptersFactoryService;
        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider _serviceProvider;
        private IProjectExplorer _projectExplorer;
        private DTE _vsEnvironment;

        public T GetService<T>()
        {
            return (T)_serviceProvider.GetService(typeof(T));
        }

        public T GetService<T>(Type serviceType)
        {
            return (T)_serviceProvider.GetService(serviceType);
        }

        public IVsEditorAdaptersFactoryService AdaptersFactoryService
        {
            get
            {
                return _adaptersFactoryService;
            }
        }

        public IProjectExplorer ProjectExplorer
        {
            get
            {
                if (_projectExplorer == null)
                    _projectExplorer = VsEnvironment != null ? new ProjectExplorer(this) : null;
                return _projectExplorer;
            }
        }

        public DTE VsEnvironment
        {
            get
            {
                if (_vsEnvironment == null)
                    _vsEnvironment = GetService<DTE>();
                return _vsEnvironment;
            }
        }

    }
}
