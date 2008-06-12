using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework.Resources;

namespace Castle.MonoRail.Views.Spark
{
    public class SparkViewData
    {
        private readonly SparkView _view;

        public SparkViewData(SparkView view)
        {
            _view = view;
        }

        public object this[string key]
        {
            get
            {
                return PropertyBag(key) ??
                    Flash(key) ??
                    Helpers(key) ??
                    Params(key) ??
                    Resources(key);
            }
        }

        object PropertyBag(string key)
        {
            return _view.PropertyBag.Contains(key) ? _view.PropertyBag[key] : null;
        }
        object Flash(string key)
        {
            return _view.Flash.Contains(key) ? _view.Flash[key] : null;
        }
        object Helpers(string key)
        {
            return _view.ControllerContext.Helpers.Contains(key) ? _view.ControllerContext.Helpers[key] : null;
        }
        object Params(string key)
        {
            return _view.Params[key];
        }
        object Resources(string key)
        {
            IResource value;
            return _view.ControllerContext.Resources.TryGetValue(key, out value) ? value : null;
        }
    }
}
