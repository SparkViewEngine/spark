using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.Parser;
using Spark.Parser.Syntax;
using Spark.Web.Mvc.Descriptors;

namespace Spark.Web.Mvc
{
    public class DefaultDescriptorBuilder : IDescriptorBuilder, ISparkServiceInitialize
    {
        private ISparkViewEngine _engine;

        public DefaultDescriptorBuilder()
            : this((string)null)
        {
        }

        public DefaultDescriptorBuilder(string _prefix)
        {
            Filters = new List<IDescriptorFilter>
                          {
                              new AreaDescriptorFilter()
                          };
            _grammar = new UseMasterGrammar(_prefix);
        }

        public DefaultDescriptorBuilder(ISparkViewEngine engine)
            : this()
        {
            _engine = engine;
            _grammar = new UseMasterGrammar(_engine.Settings.Prefix);
        }

        public virtual void Initialize(ISparkServiceContainer container)
        {
            _engine = container.GetService<ISparkViewEngine>();
            _grammar = new UseMasterGrammar(_engine.Settings.Prefix);
        }

        public IList<IDescriptorFilter> Filters { get; set; }

        public virtual IDictionary<string, object> GetExtraParameters(ControllerContext controllerContext)
        {
            var extra = new Dictionary<string, object>();
            foreach (var filter in Filters)
                filter.ExtraParameters(controllerContext, extra);
            return extra;
        }

        public virtual SparkViewDescriptor BuildDescriptor(BuildDescriptorParams buildDescriptorParams, ICollection<string> searchedLocations)
        {
            var descriptor = new SparkViewDescriptor
                                 {
                                     TargetNamespace = buildDescriptorParams.TargetNamespace
                                 };

            if (!LocatePotentialTemplate(
                     PotentialViewLocations(buildDescriptorParams.ControllerName,
                         buildDescriptorParams.ViewName,
                         buildDescriptorParams.Extra),
                     descriptor.Templates,
                     searchedLocations))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(buildDescriptorParams.MasterName))
            {
                if (!LocatePotentialTemplate(
                         PotentialMasterLocations(buildDescriptorParams.MasterName,
                             buildDescriptorParams.Extra),
                         descriptor.Templates,
                         searchedLocations))
                {
                    return null;
                }
            }
            else if (buildDescriptorParams.FindDefaultMaster && TrailingUseMasterName(descriptor) == null /*empty is a valid value*/)
            {
                LocatePotentialTemplate(
                    PotentialDefaultMasterLocations(buildDescriptorParams.ControllerName,
                        buildDescriptorParams.Extra),
                    descriptor.Templates,
                    null);
            }

            var trailingUseMaster = TrailingUseMasterName(descriptor);
            while (buildDescriptorParams.FindDefaultMaster && !string.IsNullOrEmpty(trailingUseMaster))
            {
                if (!LocatePotentialTemplate(
                         PotentialMasterLocations(trailingUseMaster,
                            buildDescriptorParams.Extra),
                         descriptor.Templates,
                         searchedLocations))
                {
                    return null;
                }
                trailingUseMaster = TrailingUseMasterName(descriptor);
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

                ParseUseMaster =
                    pos =>
                    {
                        for (var scan = pos; scan.PotentialLength() != 0; scan = scan.Advance(1))
                        {
                            var result = useMaster(scan);
                            if (result != null)
                                return result;
                        }
                        return null;
                    };
            }

            public ParseAction<string> ParseUseMaster { get; set; }
        }

        private UseMasterGrammar _grammar;
        public ParseAction<string> ParseUseMaster { get { return _grammar.ParseUseMaster; } }

        public string TrailingUseMasterName(SparkViewDescriptor descriptor)
        {
            var lastTemplate = descriptor.Templates.Last();
            var sourceContext = AbstractSyntaxProvider.CreateSourceContext(lastTemplate, _engine.ViewFolder);
            if (sourceContext == null)
                return null;
            var result = ParseUseMaster(new Position(sourceContext));
            return result == null ? null : result.Value;
        }

        private bool LocatePotentialTemplate(
            IEnumerable<string> potentialTemplates,
            ICollection<string> descriptorTemplates,
            ICollection<string> searchedLocations)
        {
            var template = potentialTemplates.FirstOrDefault(t => _engine.ViewFolder.HasView(t));
            if (template != null)
            {
                descriptorTemplates.Add(template);
                return true;
            }
            if (searchedLocations != null)
            {
                foreach (var potentialTemplate in potentialTemplates)
                    searchedLocations.Add(potentialTemplate);
            }
            return false;
        }

        private IEnumerable<string> ApplyFilters(IEnumerable<string> locations, IDictionary<string, object> extra)
        {
            // apply all of the filters PotentialLocations in order
            return Filters.Aggregate(
                locations,
                (aggregate, filter) => filter.PotentialLocations(aggregate, extra));
        }

        protected virtual IEnumerable<string> PotentialViewLocations(string controllerName, string viewName, IDictionary<string, object> extra)
        {
            return ApplyFilters(new[]
                                    {
                                        string.Format("{0}{1}{2}.spark", controllerName,Path.DirectorySeparatorChar, viewName),
                                        string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar,viewName)
                                    }, extra);
        }

        protected virtual IEnumerable<string> PotentialMasterLocations(string masterName, IDictionary<string, object> extra)
        {
            return ApplyFilters(new[]
                                    {
                                        string.Format("Layouts{0}{1}.spark", Path.DirectorySeparatorChar,masterName),
                                        string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar,masterName)
                                    }, extra);
        }

        protected virtual IEnumerable<string> PotentialDefaultMasterLocations(string controllerName, IDictionary<string, object> extra)
        {
            return ApplyFilters(new[]
                                    {
                                        string.Format("Layouts{0}{1}.spark", Path.DirectorySeparatorChar, controllerName),
                                        string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar, controllerName),
                                        string.Format("Layouts{0}Application.spark", Path.DirectorySeparatorChar),
                                        string.Format("Shared{0}Application.spark", Path.DirectorySeparatorChar)
                                    }, extra);
        }
    }
}
