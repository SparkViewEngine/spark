using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Modular.Navigation.WebPackage.Services;
using Spark.Modules;

namespace Modular.Navigation.WebPackage.Blocks
{
    public class SideBlock : IBlock
    {
        private readonly ISideRegistry _navRegistry;

        public SideBlock(ISideRegistry navRegistry)
        {
            _navRegistry = navRegistry;
        }

        public HtmlHelper Html { get; set; }

        public void RenderBlock()
        {
            Html.RenderPartial(@"Navigation\Side\Block", _navRegistry.ListItems());
        }
    }
}
