using System.Collections.Generic;
using System.Linq;

namespace Spark
{
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

        public string TargetNamespace => _targetNamespace;

        public string ControllerName => _controllerName;

        public string ViewName => _viewName;

        public string MasterName => _masterName;

        public bool FindDefaultMaster => _findDefaultMaster;

        public IDictionary<string, object> Extra => _extra;

        private static int Hash(object str)
        {
            return str?.GetHashCode() ?? 0;
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
            {
                return false;
            }

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
                if (!that._extra.TryGetValue(kv.Key, out var value) ||
                    !Equals(kv.Value, value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
