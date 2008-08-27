using System.Web;
using Spark;

namespace MediumTrustHosting
{
    public class Home : BaseHandler
    {
        public override void Process()
        {
            var view = CreateView("home.spark", "master.spark");

            view.RenderView(Context.Response.Output);
        }
    }
}
