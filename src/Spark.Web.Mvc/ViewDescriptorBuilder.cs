using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Spark.Web.Mvc
{

    public interface IViewDescriptorBuilder
    {
        SparkViewDescriptor BuildDescriptor(BuildDescriptorParams buildDescriptorParams, ICollection<string> searchedLocations);
    }

    public class BuildDescriptorParams
    {
        private readonly string _targetNamespace;
        private readonly string _areaName;
        private readonly string _controllerName;
        private readonly string _viewName;
        private readonly string _masterName;
        private readonly bool _findDefaultMaster;

        public BuildDescriptorParams(string targetNamespace, string areaName, string controllerName, string viewName, string masterName, bool findDefaultMaster)
        {
            _targetNamespace = targetNamespace;
            _areaName = areaName;
            _controllerName = controllerName;
            _viewName = viewName;
            _masterName = masterName;
            _findDefaultMaster = findDefaultMaster;
        }

        public string TargetNamespace
        {
            get { return _targetNamespace; }
        }

        public string AreaName
        {
            get { return _areaName; }
        }

        public string ControllerName
        {
            get { return _controllerName; }
        }

        public string ViewName
        {
            get { return _viewName; }
        }

        public string MasterName
        {
            get { return _masterName; }
        }

        public bool FindDefaultMaster
        {
            get { return _findDefaultMaster; }
        }
    }

    public class ViewDescriptorBuilder : IViewDescriptorBuilder, ISparkServiceInitialize
    {
        private ISparkViewEngine _engine;

        public ViewDescriptorBuilder()
        {
        }

        public ViewDescriptorBuilder(ISparkViewEngine engine)
        {
            _engine = engine;
        }

        public virtual void Initialize(ISparkServiceContainer container)
        {
            _engine = container.GetService<ISparkViewEngine>();
        }

        public virtual SparkViewDescriptor BuildDescriptor(BuildDescriptorParams buildDescriptorParams, ICollection<string> searchedLocations)
        {
            var descriptor = new SparkViewDescriptor
            {
                TargetNamespace = buildDescriptorParams.TargetNamespace
            };

            if (!LocatePotentialTemplate(
                PotentialViewLocations(buildDescriptorParams.AreaName, buildDescriptorParams.ControllerName, buildDescriptorParams.ViewName),
                descriptor.Templates,
                searchedLocations))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(buildDescriptorParams.MasterName))
            {
                if (!LocatePotentialTemplate(
                    PotentialMasterLocations(buildDescriptorParams.AreaName, buildDescriptorParams.MasterName),
                    descriptor.Templates,
                    searchedLocations))
                {
                    return null;
                }
            }
            else if (buildDescriptorParams.FindDefaultMaster && string.IsNullOrEmpty(TrailingUseMasterName(descriptor)))
            {
                LocatePotentialTemplate(
                    PotentialDefaultMasterLocations(buildDescriptorParams.AreaName, buildDescriptorParams.ControllerName),
                    descriptor.Templates,
                    null);
            }

            var trailingUseMaster = TrailingUseMasterName(descriptor);
            while(buildDescriptorParams.FindDefaultMaster && !string.IsNullOrEmpty(trailingUseMaster))
            {
                if (!LocatePotentialTemplate(
                    PotentialMasterLocations(buildDescriptorParams.AreaName, trailingUseMaster),
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
            var trailingDescriptor = new SparkViewDescriptor()
                .SetTargetNamespace(descriptor.TargetNamespace)
                .SetLanguage(descriptor.Language)
                .AddTemplate(descriptor.Templates.Last());
            var trailingEntry = _engine.CreateEntry(trailingDescriptor);
            return trailingEntry.UseMaster;
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

        protected virtual IEnumerable<string> PotentialViewLocations(string areaName, string controllerName, string viewName)
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

        protected virtual IEnumerable<string> PotentialMasterLocations(string areaName, string masterName)
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

        protected virtual IEnumerable<string> PotentialDefaultMasterLocations(string areaName, string controllerName)
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
