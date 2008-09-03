using Spark.FileSystem;

namespace Spark.Web.Mvc.Wrappers
{
    public interface IViewFolderContainer
    {
        IViewFolder ViewFolder { get; set; }
    }
}