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

namespace AspNetMvcIoC.Models
{
    public class Menu
    {
        public string Name { get; set; }
        public IList<MenuItem> Items { get; set; }
    }

    public class MenuItem
    {
        public string Caption { get; set; }
        public string Url { get; set; }
    }

    public interface INavRepository
    {
        Menu GetMenu(string name);
    }

    public class NavRepository : INavRepository
    {
        public Menu GetMenu(string name)
        {
            return new Menu
                       {
                           Name = name,
                           Items = new List<MenuItem>
                                       {
                                           new MenuItem {Caption = "Alpha", Url = "http://alpha.com"},
                                           new MenuItem {Caption = "Beta", Url = "http://beta.com"},
                                           new MenuItem {Caption = "Gamma", Url = "http://gamma.com"}
                                       }
                       };
        }
    }
}
