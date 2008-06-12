using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            get { return _view.ControllerContext.PropertyBag[key]; }
            set { }
        }
    }
}
