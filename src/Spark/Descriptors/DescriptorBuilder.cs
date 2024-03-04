using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Syntax;

namespace Spark.Descriptors
{
    public class DescriptorBuilder : IDescriptorBuilder
    {
        private IViewFolder _viewFolder;

        public DescriptorBuilder(ISparkSettings settings, IViewFolder viewFolder)
        {
            this.Filters = new List<IDescriptorFilter>
                          {
                              new AreaDescriptorFilter()
                          };

            this._grammar = new UseMasterGrammar(settings.Prefix);

            this._viewFolder = viewFolder;
        }

        public IList<IDescriptorFilter> Filters { get; set; }

        public virtual IDictionary<string, object> GetExtraParameters(SparkRouteData routeData)
        {
            var extra = new Dictionary<string, object>();
            foreach (var filter in this.Filters)
            {
                filter.ExtraParameters(routeData, extra);
            }

            return extra;
        }

        public virtual SparkViewDescriptor BuildDescriptor(BuildDescriptorParams buildDescriptorParams, ICollection<string> searchedLocations)
        {
            var descriptor = new SparkViewDescriptor
                                 {
                                     TargetNamespace = buildDescriptorParams.TargetNamespace
                                 };

            if (!this.LocatePotentialTemplate(
                     this.PotentialViewLocations(buildDescriptorParams.ControllerName,
                         buildDescriptorParams.ViewName,
                         buildDescriptorParams.Extra),
                     descriptor.Templates,
                     searchedLocations))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(buildDescriptorParams.MasterName))
            {
                if (!this.LocatePotentialTemplate(
                         this.PotentialMasterLocations(buildDescriptorParams.MasterName,
                             buildDescriptorParams.Extra),
                         descriptor.Templates,
                         searchedLocations))
                {
                    return null;
                }
            }
            else if (buildDescriptorParams.FindDefaultMaster && this.TrailingUseMasterName(descriptor) == null /*empty is a valid value*/)
            {
                this.LocatePotentialTemplate(
                    this.PotentialDefaultMasterLocations(buildDescriptorParams.ControllerName,
                        buildDescriptorParams.Extra),
                    descriptor.Templates,
                    null);
            }

            var trailingUseMaster = this.TrailingUseMasterName(descriptor);
            while (buildDescriptorParams.FindDefaultMaster && !string.IsNullOrEmpty(trailingUseMaster))
            {
                if (!this.LocatePotentialTemplate(
                         this.PotentialMasterLocations(trailingUseMaster,
                            buildDescriptorParams.Extra),
                         descriptor.Templates,
                         searchedLocations))
                {
                    return null;
                }
                trailingUseMaster = this.TrailingUseMasterName(descriptor);
            }

            return descriptor;
        }

        /// <summary>
        /// Simplified parser for &lt;use master=""/&gt; detection.
        /// TODO: get rid of this.
        /// switch to a cache of view-file to master location with iscurrent detection?
        /// </summary>
        class UseMasterGrammar : CharGrammar
        {
            public UseMasterGrammar(string _prefix)
            {
                var whiteSpace0 = Rep(Ch(char.IsWhiteSpace));
                var whiteSpace1 = Rep1(Ch(char.IsWhiteSpace));
                var startOfElement = !string.IsNullOrEmpty(_prefix) ? Ch("<" + _prefix + ":use") : Ch("<use");
                var startOfAttribute = Ch("master").And(whiteSpace0).And(Ch('=')).And(whiteSpace0);
                var attrValue = Ch('\'').And(Rep(ChNot('\''))).And(Ch('\''))
                    .Or(Ch('\"').And(Rep(ChNot('\"'))).And(Ch('\"')));

                var endOfElement = Ch("/>");

                var useMaster = startOfElement
                    .And(whiteSpace1)
                    .And(startOfAttribute)
                    .And(attrValue)
                    .And(whiteSpace0)
                    .And(endOfElement)
                    .Build(hit => new string(hit.Left.Left.Down.Left.Down.ToArray()));

                this.ParseUseMaster =
                    pos =>
                    {
                        for (var scan = pos; scan.PotentialLength() != 0; scan = scan.Advance(1))
                        {
                            var result = useMaster(scan);
                            if (result != null)
                            {
                                return result;
                            }
                        }
                        return null;
                    };
            }

            public ParseAction<string> ParseUseMaster { get; set; }
        }

        private UseMasterGrammar _grammar;
        public ParseAction<string> ParseUseMaster => this._grammar.ParseUseMaster;

        public string TrailingUseMasterName(SparkViewDescriptor descriptor)
        {
            var lastTemplate = descriptor.Templates.Last();
            var sourceContext = AbstractSyntaxProvider.CreateSourceContext(lastTemplate, this._viewFolder);
            if (sourceContext == null)
            {
                return null;
            }

            var result = this.ParseUseMaster(new Position(sourceContext));
            return result == null ? null : result.Value;
        }

        private bool LocatePotentialTemplate(
            IEnumerable<string> potentialTemplates,
            ICollection<string> descriptorTemplates,
            ICollection<string> searchedLocations)
        {
            var template = potentialTemplates.FirstOrDefault(t => this._viewFolder.HasView(t));
            if (template != null)
            {
                descriptorTemplates.Add(template);
                return true;
            }
            if (searchedLocations != null)
            {
                foreach (var potentialTemplate in potentialTemplates)
                {
                    searchedLocations.Add(potentialTemplate);
                }
            }
            return false;
        }

        protected IEnumerable<string> ApplyFilters(IEnumerable<string> locations, IDictionary<string, object> extra)
        {
            // apply all of the filters PotentialLocations in order
            return this.Filters.Aggregate(
                locations,
                (aggregate, filter) => filter.PotentialLocations(aggregate, extra));
        }

        protected virtual IEnumerable<string> PotentialViewLocations(string controllerName, string viewName, IDictionary<string, object> extra)
        {
            if (extra.TryGetValue("area", out var value))
            {
                var area = value as string;

                return this.ApplyFilters(new[]
                                        {
                                            string.Format("~{0}Areas{0}{1}{0}Views{0}{2}{0}{3}.spark", Path.DirectorySeparatorChar, area, controllerName, viewName),
                                            string.Format("~{0}Areas{0}{1}{0}Views{0}Shared{0}{2}.spark", Path.DirectorySeparatorChar, area, viewName),
                                            string.Format("{0}{1}{2}.spark", controllerName, Path.DirectorySeparatorChar, viewName),
                                            string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar, viewName),
                                            string.Format("~{0}Areas{0}{1}{0}Views{0}{2}{0}{3}.shade", Path.DirectorySeparatorChar, area, controllerName, viewName),
                                            string.Format("~{0}Areas{0}{1}{0}Views{0}Shared{0}{2}.shade", Path.DirectorySeparatorChar, area, viewName),
                                            string.Format("{0}{1}{2}.shade", controllerName, Path.DirectorySeparatorChar, viewName),
                                            string.Format("Shared{0}{1}.shade", Path.DirectorySeparatorChar, viewName)
                                        }, extra);
            }
            return this.ApplyFilters(new[]
                                    {
                                        string.Format("{0}{1}{2}.spark", controllerName,Path.DirectorySeparatorChar, viewName),
                                        string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar,viewName),
                                        string.Format("{0}{1}{2}.shade", controllerName,Path.DirectorySeparatorChar, viewName),
                                        string.Format("Shared{0}{1}.shade", Path.DirectorySeparatorChar,viewName)
                                    }, extra);
        }

        protected virtual IEnumerable<string> PotentialMasterLocations(string masterName, IDictionary<string, object> extra)
        {
            if (extra.TryGetValue("area", out var value))
            {
                var area = value as string;

                return this.ApplyFilters(new[]
                                    {
                                        string.Format("~{0}Areas{0}{1}{0}Views{0}Layouts{0}{2}.spark", Path.DirectorySeparatorChar, area, masterName),
                                        string.Format("~{0}Areas{0}{1}{0}Views{0}Shared{0}{2}.spark", Path.DirectorySeparatorChar, area, masterName),
                                        string.Format("Layouts{0}{1}.spark", Path.DirectorySeparatorChar,masterName),
                                        string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar,masterName),
                                        string.Format("~{0}Areas{0}{1}{0}Views{0}Layouts{0}{2}.shade", Path.DirectorySeparatorChar, area, masterName),
                                        string.Format("~{0}Areas{0}{1}{0}Views{0}Shared{0}{2}.shade", Path.DirectorySeparatorChar, area, masterName),
                                        string.Format("Layouts{0}{1}.shade", Path.DirectorySeparatorChar,masterName),
                                        string.Format("Shared{0}{1}.shade", Path.DirectorySeparatorChar,masterName)
                                    }, extra);
            }
            return this.ApplyFilters(new[]
                                    {
                                        string.Format("Layouts{0}{1}.spark", Path.DirectorySeparatorChar,masterName),
                                        string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar,masterName),
                                        string.Format("Layouts{0}{1}.shade", Path.DirectorySeparatorChar,masterName),
                                        string.Format("Shared{0}{1}.shade", Path.DirectorySeparatorChar,masterName)
                                    }, extra);
        }

        protected virtual IEnumerable<string> PotentialDefaultMasterLocations(string controllerName, IDictionary<string, object> extra)
        {
            if (extra.TryGetValue("area", out var value))
            {
                var area = value as string;

                return this.ApplyFilters(new[]
                                    {
                                        string.Format("~{0}Areas{0}{1}{0}Views{0}Layouts{0}{2}.spark", Path.DirectorySeparatorChar, area, controllerName),
                                        string.Format("~{0}Areas{0}{1}{0}Views{0}Shared{0}{2}.spark", Path.DirectorySeparatorChar, area, controllerName),
                                        string.Format("Layouts{0}{1}.spark", Path.DirectorySeparatorChar, controllerName),
                                        string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar, controllerName),
                                        string.Format("~{0}Areas{0}{1}{0}Views{0}Layouts{0}Application.spark", Path.DirectorySeparatorChar, area),
                                        string.Format("~{0}Areas{0}{1}{0}Views{0}Shared{0}Application.spark", Path.DirectorySeparatorChar, area),
                                        string.Format("Layouts{0}Application.spark", Path.DirectorySeparatorChar),
                                        string.Format("Shared{0}Application.spark", Path.DirectorySeparatorChar),
                                        string.Format("~{0}Areas{0}{1}{0}Views{0}Layouts{0}{2}.shade", Path.DirectorySeparatorChar, area, controllerName),
                                        string.Format("~{0}Areas{0}{1}{0}Views{0}Shared{0}{2}.shade", Path.DirectorySeparatorChar, area, controllerName),
                                        string.Format("Layouts{0}{1}.shade", Path.DirectorySeparatorChar, controllerName),
                                        string.Format("Shared{0}{1}.shade", Path.DirectorySeparatorChar, controllerName),
                                        string.Format("~{0}Areas{0}{1}{0}Views{0}Layouts{0}Application.shade", Path.DirectorySeparatorChar, area),
                                        string.Format("~{0}Areas{0}{1}{0}Views{0}Shared{0}Application.shade", Path.DirectorySeparatorChar, area),
                                        string.Format("Layouts{0}Application.shade", Path.DirectorySeparatorChar),
                                        string.Format("Shared{0}Application.shade", Path.DirectorySeparatorChar)
                                    }, extra);
            }

            return this.ApplyFilters(new[]
                                    {
                                        string.Format("Layouts{0}{1}.spark", Path.DirectorySeparatorChar, controllerName),
                                        string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar, controllerName),
                                        string.Format("Layouts{0}Application.spark", Path.DirectorySeparatorChar),
                                        string.Format("Shared{0}Application.spark", Path.DirectorySeparatorChar),
                                        string.Format("Layouts{0}{1}.shade", Path.DirectorySeparatorChar, controllerName),
                                        string.Format("Shared{0}{1}.shade", Path.DirectorySeparatorChar, controllerName),
                                        string.Format("Layouts{0}Application.shade", Path.DirectorySeparatorChar),
                                        string.Format("Shared{0}Application.shade", Path.DirectorySeparatorChar)
                                    }, extra);
        }
    }
}