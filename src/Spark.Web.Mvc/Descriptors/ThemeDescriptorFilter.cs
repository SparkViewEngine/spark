using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Spark.Web.Mvc.Descriptors
{
    public abstract class ThemeDescriptorFilter : DescriptorFilterBase
    {
        public override IEnumerable<string> PotentialLocations(IEnumerable<string> locations, IDictionary<string, object> extra)
        {
            string themeName;
            return TryGetString(extra, "theme", out themeName)
                       ? locations.Select(x => Path.Combine(string.Format("themes{0}", Path.DirectorySeparatorChar) + themeName, x)).Concat(locations)
                       : locations;
        }

        public static ThemeDescriptorFilter For(Func<ControllerContext, object> selector)
        {
            return new Delegated(selector);
        }

        class Delegated : ThemeDescriptorFilter
        {
            private readonly Func<ControllerContext, object> _selector;

            public Delegated(Func<ControllerContext, object> selector)
            {
                _selector = selector;
            }

            public override void ExtraParameters(ControllerContext context, IDictionary<string, object> extra)
            {
                var theme = Convert.ToString(_selector(context));
                if (!string.IsNullOrEmpty(theme))
                    extra["theme"] = theme;
            }
        }
    }
}