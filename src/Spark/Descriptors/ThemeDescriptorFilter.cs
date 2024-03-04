using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spark.Descriptors
{
    public abstract class ThemeDescriptorFilter : DescriptorFilterBase
    {
        public override IEnumerable<string> PotentialLocations(IEnumerable<string> locations, IDictionary<string, object> extra)
        {
            return TryGetString(extra, "theme", out var themeName)
                ? locations
                    .Select(x => Path.Combine($"themes{Path.DirectorySeparatorChar}" + themeName, x))
                    .Concat(locations)
                : locations;
        }

        public static ThemeDescriptorFilter For(Func<SparkRouteData, object> selector)
        {
            return new Delegated(selector);
        }

        class Delegated : ThemeDescriptorFilter
        {
            private readonly Func<SparkRouteData, object> _selector;

            public Delegated(Func<SparkRouteData, object> selector)
            {
                this._selector = selector;
            }

            public override void ExtraParameters(SparkRouteData context, IDictionary<string, object> extra)
            {
                var theme = Convert.ToString(this._selector(context));
                if (!string.IsNullOrEmpty(theme))
                    extra["theme"] = theme;
            }
        }
    }
}