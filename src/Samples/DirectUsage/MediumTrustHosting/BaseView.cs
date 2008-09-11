using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Spark;

namespace MediumTrustHosting
{
    /// <summary>
    /// Provide a base class for all of the views. This class is named
    /// in the web.config spark/pages/@pageTypeName setting.
    /// 
    /// Properties and methods can be added for use in spark templates.
    /// </summary>
    public abstract class BaseView : AbstractSparkView
    {
        protected BaseView()
        {
            ViewData = new ViewDataDictionary();
        }

        /// <summary>
        /// Context is assigned by BaseHandler.CreateView
        /// </summary>
        public HttpContext Context { get; set; }

        /// <summary>
        /// The generated code will use ViewData.Eval("propertyname") if 
        /// the template is using the viewdata element
        /// </summary>
        public ViewDataDictionary ViewData { get; set; }

        public class ViewDataDictionary : Dictionary<string, object>
        {
            public object Eval(string key)
            {
                return this[key];
            }
        }

        /// <summary>
        /// Provides a normalized application path
        /// </summary>
        public string SiteRoot
        {
            get
            {
                var parts = Context.Request.ApplicationPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                return string.Concat(parts.Select(part => "/" + part).ToArray());
            }
        }

        /// <summary>
        /// The generated code will use the SiteResource method when
        /// an html attribute for a url starts with ~
        /// </summary>
        public string SiteResource(string path)
        {
            return SiteRoot + path;
        }

        /// <summary>
        /// ${H(blah)} is a convenience to htmlencode the value of blah.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string H(object text)
        {
            return Context.Server.HtmlEncode(Convert.ToString(text));
        }

    }
}
