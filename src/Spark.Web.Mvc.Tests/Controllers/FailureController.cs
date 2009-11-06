using System.Web.Mvc;

namespace Spark.Web.Mvc.Tests.Controllers
{
	public class FailureController : Controller
	{
		public ActionResult Index()
		{
			return null;
		}
	}
}