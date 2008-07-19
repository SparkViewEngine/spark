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
        public StubViewData ViewData { get; set; }
    }
}