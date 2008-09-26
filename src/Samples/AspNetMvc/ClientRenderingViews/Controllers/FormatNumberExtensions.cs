using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace ClientRenderingViews.Controllers
{
    public static class FormatNumberExtensions
    {
        public static string FormatPrice(this HtmlHelper html, decimal value)
        {
            return value.ToString("#0.00");
        }
    }
}
