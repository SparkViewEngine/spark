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
    public class MenuBlock : BlockBase
    {
        private readonly INavRegistry _navRegistry;

        public MenuBlock(INavRegistry navRegistry)
        {
            _navRegistry = navRegistry;
        }

        public override void RenderBlock()
        {
            Html.RenderPartial(@"Navigation\Menu\Block", _navRegistry.ListItems());
        }
    }
}
