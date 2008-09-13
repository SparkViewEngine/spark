using Castle.MonoRail.Framework;

namespace Castle.MonoRail.Views.Spark.Wrappers
{
    public interface IViewSourceLoaderContainer
    {
        IViewSourceLoader ViewSourceLoader { get; set; }
    }
}