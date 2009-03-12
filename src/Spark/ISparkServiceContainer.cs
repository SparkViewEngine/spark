using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark
{
    public interface ISparkServiceContainer : IServiceProvider
    {
        T GetService<T>();
        void SetService<TServiceInterface>(TServiceInterface service);
        void SetServiceBuilder<TServiceInterface>(Func<ISparkServiceContainer, object> serviceBuilder);
    }
}
