using System.Web.Mvc;
using System.Web.Mvc.Html;
using Spark.Modules;

namespace Modular.Fleet.WebPackage.Blocks
{
    public class FleetTeaserBlock : BlockBase
    {
        private static int _counter;

        public override void RenderBlock()
        {
            // evil! just a demo! honest!
            _counter++;

            Html.RenderPartial(@"Fleet\Home\Teaser", new { Counter = _counter });
        }
    }
}
