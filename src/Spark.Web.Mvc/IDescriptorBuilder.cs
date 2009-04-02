using System.Collections.Generic;

namespace Spark.Web.Mvc
{
    public interface IDescriptorBuilder
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
}