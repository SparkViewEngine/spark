using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Spark.Web.Mvc;

namespace Internationalization
{
    /// <summary>
    /// Sample of providing a customized view-locations descriptor-builder
    /// </summary>
    public class CustomDescriptorBuilder : DefaultDescriptorBuilder
    {
        /// <summary>
        /// Called to quickly extract additional parameters from context
        /// </summary>
        public override IList<string> GetExtraParameters(ControllerContext controllerContext)
        {
            return new[]
                       {
                           Convert.ToString(controllerContext.HttpContext.Session["culture"])
                       };
        }

        /// <summary>
        /// This implementation relied on the default location paths and augments them with additional
        /// potention locations based on current request culture.
        /// </summary>
        protected override IEnumerable<string> PotentialViewLocations(string areaName, string controllerName, string viewName, IList<string> extra)
        {
            return Merge(extra[0], base.PotentialViewLocations(areaName, controllerName, viewName, extra));
        }

        /// <summary>
        /// same as above
        /// </summary>
        protected override IEnumerable<string> PotentialMasterLocations(string areaName, string masterName, IList<string> extra)
        {
            return Merge(extra[0], base.PotentialMasterLocations(areaName, masterName, extra));
        }

        /// <summary>
        /// same as above
        /// </summary>
        protected override IEnumerable<string> PotentialDefaultMasterLocations(string areaName, string controllerName, IList<string> extra)
        {
            return Merge(extra[0], base.PotentialDefaultMasterLocations(areaName, controllerName, extra));
        }

        /// <summary>
        /// Given any set of urls, return an enumerable set which has merged in additional possibilities.
        /// "index.spark" with culture of "en-US" for example would first match "index.en-US.spark" and
        /// then match "index.en.spark" before matching the language neutral "index.spark"
        /// </summary>
        /// <param name="culture">Whatever value was in session</param>
        /// <param name="locations">Locations passed from the default implementation</param>
        /// <returns>a yielded enumerable state machine thing. it could also just as easily 
        /// be an Array or List of string</returns>
        private static IEnumerable<string> Merge(string culture, IEnumerable<string> locations)
        {
            var dashIndex = (culture ?? "").IndexOf('-');

            var cultureExtension = string.IsNullOrEmpty(culture) ? null : culture + ".spark";
            var regionExtension = dashIndex == -1 ? null : culture.Substring(0, dashIndex) + ".spark";

            foreach (var location in locations)
            {
                if (cultureExtension != null)
                {
                    yield return Path.ChangeExtension(location, cultureExtension);
                    if (regionExtension != null)
                        yield return Path.ChangeExtension(location, regionExtension);
                }
                yield return location;
            }
        }
    }
}