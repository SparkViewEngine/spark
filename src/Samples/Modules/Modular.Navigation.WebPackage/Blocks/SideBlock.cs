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
    public class SideBlock : BlockBase
    {
        private readonly ISideRegistry _navRegistry;

        public SideBlock(ISideRegistry navRegistry)
        {
            _navRegistry = navRegistry;
        }

        public override void RenderBlock()
        {
            Html.RenderPartial(@"Navigation\Side\Block", _navRegistry.ListItems());
        }
    }
}
