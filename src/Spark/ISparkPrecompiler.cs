using System.Reflection;

namespace Spark;

public interface ISparkPrecompiler
{
    Assembly Precompile(SparkBatchDescriptor batch);
}