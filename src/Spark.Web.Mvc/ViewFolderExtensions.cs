using System.Reflection;
using Spark.FileSystem;
using Spark.Web.Mvc.Wrappers;

namespace Spark.Web.Mvc
{
    public static class ViewFolderExtensions
    {
        public static void AddSharedPath(this IViewFolderContainer viewFolderContainer, string virtualPath)
        {
            viewFolderContainer.ViewFolder = viewFolderContainer.ViewFolder.AddSharedPath(virtualPath);
        }

        public static void AddLayoutsPath(this IViewFolderContainer viewFolderContainer, string virtualPath)
        {
            viewFolderContainer.ViewFolder = viewFolderContainer.ViewFolder.AddLayoutsPath(virtualPath);
        }

        public static void AddEmbeddedResources(this IViewFolderContainer viewFolderContainer, Assembly assembly, string resourcePath)
        {
            viewFolderContainer.ViewFolder =
                viewFolderContainer.ViewFolder.Append(new EmbeddedViewFolder(assembly, resourcePath));
        }
    }
}
