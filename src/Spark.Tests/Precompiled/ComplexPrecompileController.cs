namespace Spark.Tests.Precompiled
{
    [Precompile(Exclude = "Show*", Layout = "Default")]
    [Precompile(Include = "_Foo _Bar", Layout = "Ajax")]
    [Precompile(Include = "Show*", Layout = "Showing")]
    public class ComplexPrecompileController 
    {
    }
}
