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
using Benchmark;
using Castle.MonoRail.Framework;

namespace BenchmarkVelocity.Controllers
{
    [Layout("default")]
    public class HomeController : SmartDispatcherController
    {
        static BlogDao dao = new BlogDao();

        public void Index()
        {
            PropertyBag["Post"] = dao.GetPost();
        }
    }
}