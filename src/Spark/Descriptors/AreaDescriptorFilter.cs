using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spark.Descriptors
{
    public class AreaDescriptorFilter : DescriptorFilterBase
    {
        public override void ExtraParameters(SparkRouteData routeData, IDictionary<string, object> extra)
        {
            var areaName = GetAreaName(routeData);
            if (!string.IsNullOrEmpty(areaName))
                extra["area"] = areaName;
        }

        public override IEnumerable<string> PotentialLocations(IEnumerable<string> locations, IDictionary<string, object> extra)
        {
            return TryGetString(extra, "area", out var areaName)
                       ? locations.Select(x => Path.Combine(areaName, x)).Concat(locations)
                       : locations;
        }

        private static string GetAreaName(SparkRouteData routeData)
        {
            if (routeData.Values.TryGetValue("area", out var area))
            {
                return area as string;
            }
            if (routeData.Values.TryGetValue("area", out area))
            {
                return area as string;
            }

            return null;
        }
    }
}
