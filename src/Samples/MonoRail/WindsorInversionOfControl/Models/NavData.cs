using System;
using System.Collections.Generic;
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

namespace WindsorInversionOfControl.Models
{
    public class NavData
    {
        public IList<NavItem> Items { get; set; }
    }

    public class NavItem
    {
        public string Caption { get; set; }
        public string Url { get; set; }
    }
}
