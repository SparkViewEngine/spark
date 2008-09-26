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
using System.Web.Mvc;

namespace NorthwindDemo.Views.Helpers {
    public static class ValidationHelpers 
    {
        public static string Label(this HtmlHelper html, string text, string name) 
        {
            string css = string.Empty;

            if (html.ViewContext.ViewData.ContainsKey("Error:" + name)) {
                css = " class=\"error\"";
            }

            string format = "<label for=\"{0}\"{2}>{1}</label>";
            return string.Format(format, name, text, css);
        }
    }
}
