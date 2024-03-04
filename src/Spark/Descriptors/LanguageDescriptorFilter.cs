using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spark.Descriptors
{
    public abstract class LanguageDescriptorFilter : DescriptorFilterBase
    {
        public override IEnumerable<string> PotentialLocations(IEnumerable<string> locations, IDictionary<string, object> extra)
        {
            if (!TryGetString(extra, "language", out var languageName))
            {
                return locations;
            }

            var extension = languageName + ".spark";

            var slashPos = languageName.IndexOf('-');
            if (slashPos == -1)
            {
                return locations.SelectMany(
                    path => new[]
                             {
                                 Path.ChangeExtension(path, extension),
                                 path
                             });
            }

            var shortExtension = languageName.Substring(0, slashPos) + ".spark";
            return locations.SelectMany(
                    path => new[]
                             {
                                 Path.ChangeExtension(path, extension),
                                 Path.ChangeExtension(path, shortExtension),
                                 path
                             });
        }

        public static LanguageDescriptorFilter For(Func<SparkRouteData, string> selector)
        {
            return new Delegated(selector);
        }

        class Delegated : LanguageDescriptorFilter
        {
            private readonly Func<SparkRouteData, string> _selector;

            public Delegated(Func<SparkRouteData, string> selector)
            {
                this._selector = selector;
            }

            public override void ExtraParameters(SparkRouteData context, IDictionary<string, object> extra)
            {
                var theme = Convert.ToString(this._selector(context));
                if (!string.IsNullOrEmpty(theme))
                {
                    extra["language"] = theme;
                }
            }
        }
    }
}
