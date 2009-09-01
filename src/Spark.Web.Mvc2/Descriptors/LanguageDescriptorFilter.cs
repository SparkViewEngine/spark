using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Spark.Web.Mvc.Descriptors
{
    public abstract class LanguageDescriptorFilter : DescriptorFilterBase
    {
        public override IEnumerable<string> PotentialLocations(IEnumerable<string> locations, IDictionary<string, object> extra)
        {
            string languageName;
            if (!TryGetString(extra, "language", out languageName))
                return locations;

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

        public static LanguageDescriptorFilter For(Func<ControllerContext, string> selector)
        {
            return new Delegated(selector);
        }

        class Delegated : LanguageDescriptorFilter
        {
            private readonly Func<ControllerContext, string> _selector;

            public Delegated(Func<ControllerContext, string> selector)
            {
                _selector = selector;
            }

            public override void ExtraParameters(ControllerContext context, IDictionary<string, object> extra)
            {
                var theme = Convert.ToString(_selector(context));
                if (!string.IsNullOrEmpty(theme))
                    extra["language"] = theme;
            }

        }
    }
}
