using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace PartialFiles
{
    /// <summary>
    /// Default ViewPage base class for aspx files in this project.
    /// Set via system.web/pages/@pageBaseType
    /// </summary>
    public class ViewPage : System.Web.Mvc.ViewPage
    {
        /// <summary>
        /// Must substitute the ViewContext TextWriter, because 
        /// the native HttpContext.Current TextWriter will be used by default
        /// </summary>
        protected override HtmlTextWriter CreateHtmlTextWriter(System.IO.TextWriter tw)
        {
            if (ViewContext != null)
                return base.CreateHtmlTextWriter(ViewContext.HttpContext.Response.Output);
            
            return base.CreateHtmlTextWriter(tw);
        }
    }
}
