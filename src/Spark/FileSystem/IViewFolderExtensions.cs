using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Spark.FileSystem
{
    public static class IViewFolderExtensions
    {
        public static IViewFolder Append(this IViewFolder viewFolder, IViewFolder additional)
        {
            return new CombinedViewFolder(viewFolder, additional);
        }

        public static IViewFolder AddSharedPath(this IViewFolder viewFolder, string virtualPath)
        {
            var vppFolder = new SubViewFolder(new VirtualPathProviderViewFolder(virtualPath), "Shared");
            return Append(viewFolder, vppFolder);
        }

        public static IViewFolder AddLayoutsPath(this IViewFolder viewFolder, string virtualPath)
        {
            var vppFolder = new SubViewFolder(new VirtualPathProviderViewFolder(virtualPath), "Layouts");
            return Append(viewFolder, vppFolder);
        }

        public static IViewFolder ApplySettings(this IViewFolder viewFolder, ISparkSettings settings)
        {
            return viewFolder;
        }
    }
}
