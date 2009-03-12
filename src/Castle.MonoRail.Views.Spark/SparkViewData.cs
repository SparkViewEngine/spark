// Copyright 2008 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System.Collections.Generic;

namespace Castle.MonoRail.Views.Spark
{
    using Castle.MonoRail.Framework.Resources;
    using System.Linq;

    public class SparkViewData
    {
        readonly SparkView _view;
        private Dictionary<string, object> _params;

        public SparkViewData(SparkView view)
        {
            _view = view;
        }

        public bool TryGetViewData(string key, out object value)
        {
            return
                TryPropertyBag(key, out value) ||
                TryFlash(key, out value) ||
                TryHelpers(key, out value) ||
                TryParams(key, out value) ||
                TryResources(key, out value);
        }

        public object Eval(string key)
        {
            object value;
            return TryGetViewData(key, out value) ? value : null;
        }

        public object this[string key]
        {
            get
            {
                object value;
                return TryGetViewData(key, out value) ? value : null;
            }
        }

        bool TryPropertyBag(string key, out object value)
        {
            var containsKey = _view.PropertyBag.Contains(key);
            value = containsKey ? _view.PropertyBag[key] : null;
            return containsKey;
        }
        bool TryFlash(string key, out object value)
        {
            var containsKey = _view.Flash.ContainsKey(key);
            value = containsKey ? _view.Flash[key] : null;
            return containsKey;
        }
        bool TryHelpers(string key, out object value)
        {
            var containsKey = _view.ControllerContext.Helpers.Contains(key);
            value = containsKey ? _view.ControllerContext.Helpers[key] : null;
            return containsKey;
        }
        bool TryParams(string key, out object value)
        {
            if (_params == null)
            {
                _params = new Dictionary<string, object>();
                foreach (var name in _view.Params.AllKeys)
                    _params[name] = _view.Params[name];
            }
            return _params.TryGetValue(key, out value);
        }
        bool TryResources(string key, out object value)
        {
            IResource resource;
            if (_view.ControllerContext.Resources.TryGetValue(key, out resource))
            {
                value = resource;
                return true;
            }
            value = null;
            return false;
        }

    }
}
