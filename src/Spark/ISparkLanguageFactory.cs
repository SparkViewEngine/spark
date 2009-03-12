using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler;

namespace Spark
{
    public interface ISparkLanguageFactory
    {
        ViewCompiler CreateViewCompiler(ISparkViewEngine engine, SparkViewDescriptor descriptor);
        void InstanceCreated(ViewCompiler compiler, ISparkView view);
        void InstanceReleased(ViewCompiler compiler, ISparkView view);
    }
}
