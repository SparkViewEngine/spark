using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Spark.Web.Mvc.Descriptors
{
    public class AreaDescriptorFilter : DescriptorFilterBase
    {
        public override void ExtraParameters(ControllerContext context, IDictionary<string, object> extra)
        {
            var areaName = GetAreaName(context.RouteData);
            if (!string.IsNullOrEmpty(areaName))
                extra["area"] = areaName;
        }

        public override IEnumerable<string> PotentialLocations(IEnumerable<string> locations, IDictionary<string, object> extra)
        {
            string areaName;

            return TryGetString(extra, "area", out areaName)
                       ? locations.Select(x => Path.Combine(areaName, x)).Concat(locations)
                       : locations;
        }


        private static string GetAreaName(RouteBase route)
        {
            var routeWithArea = route as IRouteWithArea;
            if (routeWithArea != null)
            {
                return routeWithArea.Area;
            }

            var castRoute = route as Route;
            if (castRoute != null && castRoute.DataTokens != null)
            {
                return castRoute.DataTokens["area"] as string;
            }

            return null;
        }

        private static string GetAreaName(RouteData routeData)
        {
            object area;
            if (routeData.DataTokens.TryGetValue("area", out area))
            {
                return area as string;
            }
            if (routeData.Values.TryGetValue("area", out area))
            {
                return area as string;
            }

            return GetAreaName(routeData.Route);
        }

    }
}
