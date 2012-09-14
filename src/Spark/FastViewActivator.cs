using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Spark
{
    public class FastViewActivator : IViewActivatorFactory, IViewActivator
    {
        private static Dictionary<Type, Func<ISparkView>> _ctors = new Dictionary<Type, Func<ISparkView>>();

        public IViewActivator Register(Type type)
        {
            //you could put a guard clause in here too to ensure the type can be cast to ISparkView
            if (_ctors.ContainsKey(type)) return this;

            var exp = Expression.New(type);
            var d = Expression.Lambda<Func<ISparkView>>(exp).Compile();
            _ctors.Add(type, d);

            return this;
        }

        public void Unregister(Type type, IViewActivator activator)
        {
        }

        public ISparkView Activate(Type type)
        {
            return _ctors[type]();
        }

        public void Release(Type type, ISparkView view)
        {
        }
    }
}