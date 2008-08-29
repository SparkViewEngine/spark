using System;
using Castle.Core;
using Castle.MicroKernel;
using Spark;

namespace WindsorInversionOfControl
{
    public class ViewActivator : IViewActivatorFactory, IViewActivator
    {
        private readonly IKernel kernel;

        public ViewActivator(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IViewActivator Register(Type type)
        {
            kernel.AddComponent(type.FullName, typeof(ISparkView), type, LifestyleType.Transient);
            return this;
        }

        public void Unregister(Type type, IViewActivator activator)
        {
            kernel.RemoveComponent(type.FullName);
        }

        public ISparkView Activate(Type type)
        {
            return kernel.Resolve<ISparkView>(type.FullName);
        }

        public void Release(Type type, ISparkView view)
        {
            kernel.ReleaseComponent(view);
        }
    }
}