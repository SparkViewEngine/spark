using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.Web.Mvc.Descriptors;

namespace Spark.Web.Mvc
{
    public class DefaultDescriptorBuilder : IDescriptorBuilder, ISparkServiceInitialize
    {
        private ISparkViewEngine _engine;

        public DefaultDescriptorBuilder()
        {
            Filters = new List<IDescriptorFilter>
                          {
                              new AreaDescriptorFilter()
                          };
        }

        public DefaultDescriptorBuilder(ISparkViewEngine engine)
            : this()
        {
            _engine = engine;
        }

        public virtual void Initialize(ISparkServiceContainer container)
        {
            _engine = container.GetService<ISparkViewEngine>();
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
            else if (buildDescriptorParams.FindDefaultMaster && string.IsNullOrEmpty(TrailingUseMasterName(descriptor)))
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

        public string TrailingUseMasterName(SparkViewDescriptor descriptor)
        {
            var context = new VisitorContext
                          {
                              ViewFolder = _engine.ViewFolder,
                              SyntaxProvider = _engine.SyntaxProvider,
                              ViewPath = descriptor.Templates.Last()
                          };
            var chunks = _engine.SyntaxProvider.GetChunks(context, context.ViewPath);
            var useMasterChunks = chunks.OfType<UseMasterChunk>();
            return useMasterChunks.Any() ? useMasterChunks.First().Name : null;
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
                                        controllerName + "\\" + viewName + ".spark",
                                        "Shared\\" + viewName + ".spark"
                                    }, extra);
        }

        protected virtual IEnumerable<string> PotentialMasterLocations(string masterName, IDictionary<string, object> extra)
        {
            return ApplyFilters(new[]
                                    {
                                        "Layouts\\" + masterName + ".spark",
                                        "Shared\\" + masterName + ".spark"
                                    }, extra);
        }

        protected virtual IEnumerable<string> PotentialDefaultMasterLocations(string controllerName, IDictionary<string, object> extra)
        {
            return ApplyFilters(new[]
                                    {
                                        "Layouts\\" + controllerName + ".spark",
                                        "Shared\\" + controllerName + ".spark",
                                        "Layouts\\Application.spark",
                                        "Shared\\Application.spark"
                                    }, extra);
        }
    }
}
