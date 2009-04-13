using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Spark.Web.Mvc
{
    public interface IDescriptorBuilder
    {
        /// <summary>
        /// Implemented by custom descriptor builder to quickly extract additional parameters needed
        /// to locate templates, like the theme or language in effect for the request
        /// </summary>
        /// <param name="controllerContext">Context information for the current request</param>
        /// <returns>An in-order array of values which are meaningful to BuildDescriptor on the same implementation class</returns>
        IDictionary<string, object> GetExtraParameters(ControllerContext controllerContext);

        /// <summary>
        /// Given a set of MVC-specific parameters, a descriptor for the target view is created. This can
        /// be a bit more expensive because the existence of files is tested at various candidate locations.
        /// </summary>
        /// <param name="buildDescriptorParams">Contains all of the standard and extra parameters which contribute to a descriptor</param>
        /// <param name="searchedLocations">Candidate locations are added to this collection so an information-rich error may be returned</param>
        /// <returns>The descriptor with all of the detected view locations in order</returns>
        SparkViewDescriptor BuildDescriptor(BuildDescriptorParams buildDescriptorParams, ICollection<string> searchedLocations);
    }

    public class BuildDescriptorParams
    {
        private readonly string _targetNamespace;
        private readonly string _controllerName;
        private readonly string _viewName;
        private readonly string _masterName;
        private readonly bool _findDefaultMaster;
        private readonly IDictionary<string, object> _extra;
        private static readonly IDictionary<string, object> _extraEmpty = new Dictionary<string, object>();
        private readonly int _hashCode;

        public BuildDescriptorParams(string targetNamespace, string controllerName, string viewName, string masterName, bool findDefaultMaster, IDictionary<string, object> extra)
        {
            _targetNamespace = targetNamespace;
            _controllerName = controllerName;
            _viewName = viewName;
            _masterName = masterName;
            _findDefaultMaster = findDefaultMaster;
            _extra = extra ?? _extraEmpty;

            // this object is meant to be immutable and used in a dictionary.
            // the hash code will always be used so it isn't calculated just-in-time.
            _hashCode = CalculateHashCode();
        }

        public string TargetNamespace
        {
            get { return _targetNamespace; }
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

        public IDictionary<string, object> Extra
        {
            get { return _extra; }
        }

        private static int Hash(object str)
        {
            return str == null ? 0 : str.GetHashCode();
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        private int CalculateHashCode()
        {
            return Hash(_viewName) ^
                   Hash(_controllerName) ^
                   Hash(_targetNamespace) ^
                   Hash(_masterName) ^
                   _findDefaultMaster.GetHashCode() ^
                   _extra.Aggregate(0, (hash, kv) => hash ^ Hash(kv.Key) ^ Hash(kv.Value));
        }

        public override bool Equals(object obj)
        {
            var that = obj as BuildDescriptorParams;
            if (that == null || that.GetType() != GetType())
                return false;

            if (!string.Equals(_viewName, that._viewName) ||
                !string.Equals(_controllerName, that._controllerName) ||
                !string.Equals(_targetNamespace, that._targetNamespace) ||
                !string.Equals(_masterName, that._masterName) ||
                _findDefaultMaster != that._findDefaultMaster ||
                _extra.Count != that._extra.Count)
            {
                return false;
            }
            foreach (var kv in _extra)
            {
                object value;
                if (!that._extra.TryGetValue(kv.Key, out value) ||
                    !Equals(kv.Value, value))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
