using System.IO;
using Spark.FileSystem;

namespace Spark
{
    public static class SparkSettingsExtensions
    {
        public static IViewFolder CreateDefaultViewFolder(this ISparkSettings settings)
        {
            var viewLocation = Path.Combine(settings.RootPath, "Views");

            return new FileSystemViewFolder(viewLocation);
        }
    }
}