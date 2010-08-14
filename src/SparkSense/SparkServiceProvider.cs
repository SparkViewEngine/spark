using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

namespace SparkSense
{
    public interface ISparkServiceProvider
    {
        T GetService<T>();
        T GetService<T>(Type serviceType);
        DTE VsEnvironment { get; }
    }

    [Export(typeof(ISparkServiceProvider))]
    public class SparkServiceProvider : ISparkServiceProvider
    {
        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider _serviceProvider;
        private DTE _vsEnvironment;

        public T GetService<T>()
        {
            return (T)_serviceProvider.GetService(typeof(T));
        }

        public T GetService<T>(Type serviceType)
        {
            return (T)_serviceProvider.GetService(serviceType);
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
