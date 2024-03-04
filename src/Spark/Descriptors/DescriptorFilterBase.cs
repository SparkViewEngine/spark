using System;
using System.Collections.Generic;

namespace Spark.Descriptors
{
    public abstract class DescriptorFilterBase : IDescriptorFilter
    {
        public abstract void ExtraParameters(SparkRouteData routeData, IDictionary<string, object> extra);

        public abstract IEnumerable<string> PotentialLocations(
            IEnumerable<string> locations,
            IDictionary<string, object> extra);

        protected static bool TryGetString(IDictionary<string, object> extra, string name, out string value)
        {
            if (extra.TryGetValue(name, out var obj))
            {
                value = Convert.ToString(obj);
                return !string.IsNullOrEmpty(value);
            }

            value = null;

            return false;
        }
    }
}
