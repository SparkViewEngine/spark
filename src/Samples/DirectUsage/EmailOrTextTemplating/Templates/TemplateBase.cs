using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spark;

namespace EmailOrTextTemplating.Templates
{
    /// <summary>
    /// Base class of all spark views. In this example it's named in the
    /// web.config spark/pages/@pageBaseType attribute. 
    /// 
    /// If you use #latebound syntax in expressions you need to have Eval
    /// methods in the base class, and with direct usage it's a
    /// "bring your own Eval" situation.
    /// 
    /// For convenience this example will rely on the ViewDataDictionary.
    /// </summary>
    public abstract class TemplateBase : AbstractSparkView
    {
        public ViewDataDictionary ViewData { get; set; }

        public object Eval(string expression)
        {
            return ViewData.Eval(expression);
        }

        public string Eval(string expression, string format)
        {
            return ViewData.Eval(expression, format);
        }

        /// <summary>
        /// Members of this class are also available to the views
        /// </summary>
        public bool IsInStock(int productId)
        {
            return DateTime.UtcNow.Second % 2 == 1;
        }
    }
}
