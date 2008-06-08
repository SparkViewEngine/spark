using System.Text;

namespace Spark.Tests.Stubs
{
	public class StubViewContext
	{
		public string ControllerName { get; set; }
		public string ViewName { get; set; }
		public string MasterName { get; set; }

		public StringBuilder Output { get; set; }
	}
}