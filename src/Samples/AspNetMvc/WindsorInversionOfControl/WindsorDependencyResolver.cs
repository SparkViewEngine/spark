using System;
using Castle.MicroKernel;

namespace WindsorInversionOfControl
{
    /// <summary>
    /// Provides Windsor's Kernel capabilities to MvcContrib's IoC infrastructure
    /// </summary>
    public class WindsorDependencyResolver : MvcContrib.Interfaces.IDependencyResolver
    {
        private readonly IKernel _kernel;

        public WindsorDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        public void DisposeImplementation(object instance)
        {
            _kernel.ReleaseComponent(instance);
        }

        public Interface GetImplementationOf<Interface>()
        {
            return (Interface)GetImplementationOf(typeof(Interface));
        }

        public Interface GetImplementationOf<Interface>(Type type)
        {
            return (Interface)GetImplementationOf(type);
        }

        public object GetImplementationOf(Type type)
        {
            if (_kernel.HasComponent(type))
            {
                return _kernel.Resolve(type);
            }
            return null;
        }
    }
}