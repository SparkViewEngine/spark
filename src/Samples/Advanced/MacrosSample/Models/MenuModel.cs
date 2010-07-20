using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace MacrosSample.Models {
    public class MenuModel {
        public string Caption { get; set; }
        public RouteValueDictionary RouteValues { get; set; }
        public IEnumerable<MenuModel> Children { get; set; }
    }
}
