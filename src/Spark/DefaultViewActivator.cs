using System;

namespace Spark
{
    public class DefaultViewActivator : IViewActivatorFactory, IViewActivator
    {
        public IViewActivator Register(Type type)
        {
            return this;
        }

        public void Unregister(Type type, IViewActivator activator)
        {
        }

        public ISparkView Activate(Type type)
        {
            return (ISparkView)Activator.CreateInstance(type);
        }

        public void Release(Type type, ISparkView view)
        {
        }
    }
}