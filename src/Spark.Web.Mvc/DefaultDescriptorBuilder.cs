using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;

namespace Spark.Web.Mvc
{
    public class DefaultDescriptorBuilder : IDescriptorBuilder, ISparkServiceInitialize
    {
        private ISparkViewEngine _engine;

        public DefaultDescriptorBuilder()
        {
        }

        public DefaultDescriptorBuilder(ISparkViewEngine engine)
        {
            _engine = engine;
        }

        public virtual void Initialize(ISparkServiceContainer container)
        {
            _engine = container.GetService<ISparkViewEngine>();
        }

        public virtual IList<string> GetExtraParameters(ControllerContext controllerContext)
        {
            return null;
        }

        public virtual SparkViewDescriptor BuildDescriptor(BuildDescriptorParams buildDescriptorParams, ICollection<string> searchedLocations)
        {
            var descriptor = new SparkViewDescriptor
                                 {
                                     TargetNamespace = buildDescriptorParams.TargetNamespace
                                 };

            if (!LocatePotentialTemplate(
                     PotentialViewLocations(
                         buildDescriptorParams.AreaName,
                         buildDescriptorParams.ControllerName,
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
                         PotentialMasterLocations(
                             buildDescriptorParams.AreaName,
                             buildDescriptorParams.MasterName,
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
                    PotentialDefaultMasterLocations(
                        buildDescriptorParams.AreaName,
                        buildDescriptorParams.ControllerName,
                        buildDescriptorParams.Extra),
                    descriptor.Templates,
                    null);
            }

            var trailingUseMaster = TrailingUseMasterName(descriptor);
            while (buildDescriptorParams.FindDefaultMaster && !string.IsNullOrEmpty(trailingUseMaster))
            {
                if (!LocatePotentialTemplate(
                         PotentialMasterLocations(
                            buildDescriptorParams.AreaName,
                            trailingUseMaster,
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

        protected virtual IEnumerable<string> PotentialViewLocations(string areaName, string controllerName, string viewName, IList<string> extra)
        {
            return string.IsNullOrEmpty(areaName)
                       ? new[]
                             {
                                 controllerName + "\\" + viewName + ".spark",
                                 "Shared\\" + viewName + ".spark"
                             }
                       : new[]
                             {
                                 areaName + "\\" + controllerName + "\\" + viewName + ".spark",
                                 controllerName + "\\" + viewName + ".spark",
                                 "Shared\\" + viewName + ".spark"
                             };
        }

        protected virtual IEnumerable<string> PotentialMasterLocations(string areaName, string masterName, IList<string> extra)
        {
            return string.IsNullOrEmpty(areaName)
                       ? new[]
                             {
                                 "Layouts\\" + masterName + ".spark",
                                 "Shared\\" + masterName + ".spark"
                             }
                       : new[]
                             {
                                 areaName + "\\Layouts\\" + masterName + ".spark",
                                 areaName + "\\Shared\\" + masterName + ".spark",
                                 "Layouts\\" + masterName + ".spark",
                                 "Shared\\" + masterName + ".spark"
                             };
        }

        protected virtual IEnumerable<string> PotentialDefaultMasterLocations(string areaName, string controllerName, IList<string> extra)
        {
            return string.IsNullOrEmpty(areaName)
                       ? new[]
                             {
                                 "Layouts\\" + controllerName + ".spark",
                                 "Shared\\" + controllerName + ".spark",
                                 "Layouts\\Application.spark",
                                 "Shared\\Application.spark"
                             }
                       : new[]
                             {
                                 areaName + "\\Layouts\\" + controllerName + ".spark",
                                 areaName + "\\Shared\\" + controllerName + ".spark",
                                 areaName + "\\Layouts\\Application.spark",
                                 areaName + "\\Shared\\Application.spark",
                                 "Layouts\\" + controllerName + ".spark",
                                 "Shared\\" + controllerName + ".spark",
                                 "Layouts\\Application.spark",
                                 "Shared\\Application.spark"
                             };
        }
    }
}
