using System;

namespace Spark
{
    public interface IViewActivatorFactory
    {
        IViewActivator Register(Type type);
        void Unregister(Type type, IViewActivator activator);
    }

    public interface IViewActivator
    {
        ISparkView Activate(Type type);
        void Release(Type type, ISparkView view);
    }
}

