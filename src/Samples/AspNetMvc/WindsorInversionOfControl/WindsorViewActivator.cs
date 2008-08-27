using System;
using Castle.Core;
using Castle.MicroKernel;
using Spark;

namespace WindsorInversionOfControl
{
    public class WindsorViewActivator : IViewActivatorFactory
    {
        private readonly IKernel kernel;

        public WindsorViewActivator(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IViewActivator Register(Type type)
        {
            kernel.AddComponent(type.AssemblyQualifiedName, typeof(ISparkView), type, LifestyleType.Transient);
            return new Activator(kernel, type.AssemblyQualifiedName);
        }

        public void Unregister(Type type, IViewActivator activator)
        {
            kernel.RemoveComponent(type.AssemblyQualifiedName);            
        }

        class Activator : IViewActivator
        {
            private readonly IKernel kernel;
            private readonly string key;

            public Activator(IKernel kernel, string key)
            {
                this.kernel = kernel;
                this.key = key;
            }

            public ISparkView Activate(Type type)
            {
                return (ISparkView)kernel.Resolve(key, typeof(ISparkView));
            }

            public void Release(Type type, ISparkView view)
            {
                kernel.ReleaseComponent(view);
            }
        }
    }
}