using Spark.Descriptors;
using Spark.FileSystem;

namespace Spark.AspNetCore.Mvc;

public class AspNetCoreDescriptorBuilder : DescriptorBuilder
{
    public AspNetCoreDescriptorBuilder(ISparkSettings settings, IViewFolder viewFolder) : base(settings, viewFolder)
    {
    }

    protected override IEnumerable<string> PotentialViewLocations(string controllerName, string viewName, IDictionary<string, object> extra)
    {
        if (extra.TryGetValue("area", out var value))
        {
            var area = value as string;

            return ApplyFilters([
                $"Areas{Path.DirectorySeparatorChar}{area}{Path.DirectorySeparatorChar}Views{Path.DirectorySeparatorChar}{controllerName}{Path.DirectorySeparatorChar}{viewName}.spark",
                $"Areas{Path.DirectorySeparatorChar}{area}{Path.DirectorySeparatorChar}Views{Path.DirectorySeparatorChar}Shared{Path.DirectorySeparatorChar}{viewName}.spark",
                $"{controllerName}{Path.DirectorySeparatorChar}{viewName}.spark",
                $"Shared{Path.DirectorySeparatorChar}{viewName}.spark"
            ], extra);
        }

        return ApplyFilters([
            $"{controllerName}{Path.DirectorySeparatorChar}{viewName}.spark",
            $"Shared{Path.DirectorySeparatorChar}{viewName}.spark"
        ], extra);
    }

    protected override IEnumerable<string> PotentialMasterLocations(string masterName, IDictionary<string, object> extra)
    {
        if (extra.TryGetValue("area", out var value))
        {
            var area = value as string;

            return ApplyFilters([
                $"Areas{Path.DirectorySeparatorChar}{area}{Path.DirectorySeparatorChar}Views{Path.DirectorySeparatorChar}Layouts{Path.DirectorySeparatorChar}{masterName}.spark",
                $"Areas{Path.DirectorySeparatorChar}{area}{Path.DirectorySeparatorChar}Views{Path.DirectorySeparatorChar}Shared{Path.DirectorySeparatorChar}{masterName}.spark",
                $"Layouts{Path.DirectorySeparatorChar}{masterName}.spark",
                $"Shared{Path.DirectorySeparatorChar}{masterName}.spark"
            ], extra);
        }

        return ApplyFilters([
            $"Layouts{Path.DirectorySeparatorChar}{masterName}.spark",
            $"Shared{Path.DirectorySeparatorChar}{masterName}.spark"
        ], extra);
    }

    protected override IEnumerable<string> PotentialDefaultMasterLocations(string controllerName, IDictionary<string, object> extra)
    {
        if (extra.TryGetValue("area", out var value))
        {
            var area = value as string;

            return ApplyFilters([
                $"Areas{Path.DirectorySeparatorChar}{area}{Path.DirectorySeparatorChar}Views{Path.DirectorySeparatorChar}Layouts{Path.DirectorySeparatorChar}{controllerName}.spark",
                $"Areas{Path.DirectorySeparatorChar}{area}{Path.DirectorySeparatorChar}Views{Path.DirectorySeparatorChar}Shared{Path.DirectorySeparatorChar}{controllerName}.spark",
                $"Layouts{Path.DirectorySeparatorChar}{controllerName}.spark",
                $"Shared{Path.DirectorySeparatorChar}{controllerName}.spark",
                $"Areas{Path.DirectorySeparatorChar}{area}{Path.DirectorySeparatorChar}Views{Path.DirectorySeparatorChar}Layouts{Path.DirectorySeparatorChar}Application.spark",
                $"Areas{Path.DirectorySeparatorChar}{area}{Path.DirectorySeparatorChar}Views{Path.DirectorySeparatorChar}Shared{Path.DirectorySeparatorChar}Application.spark",
                $"Layouts{Path.DirectorySeparatorChar}Application.spark",
                $"Shared{Path.DirectorySeparatorChar}Application.spark",
            ], extra);
        }

        return ApplyFilters([
            $"Layouts{Path.DirectorySeparatorChar}{controllerName}.spark",
            $"Shared{Path.DirectorySeparatorChar}{controllerName}.spark",
            $"Layouts{Path.DirectorySeparatorChar}Application.spark",
            $"Shared{Path.DirectorySeparatorChar}Application.spark"
        ], extra);
    }
}