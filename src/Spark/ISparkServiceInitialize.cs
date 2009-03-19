using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark
{
    public interface ISparkServiceInitialize
    {
        void Initialize(ISparkServiceContainer container);
    }
}
