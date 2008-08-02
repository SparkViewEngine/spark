using System.Collections.Generic;

namespace Spark.Tests.Stubs
{
    public class StubViewData : Dictionary<string, object>
    {
        public object Eval(string key)
        {
            return this[key];
        }
    }

    public abstract class StubSparkView : AbstractSparkView
    {
        public StubSparkView()
        {
            ViewData = new StubViewData();
        }

        public StubViewData ViewData { get; set; }

        public string SiteRoot { get { return "/TestApp"; } }
    }
}