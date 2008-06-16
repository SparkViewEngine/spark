using System.Collections.Generic;

namespace Spark.Tests.Stubs
{
	public abstract class StubSparkView : AbstractSparkView
	{
        public IDictionary<string, object> ViewData { get; set; }
	}
}