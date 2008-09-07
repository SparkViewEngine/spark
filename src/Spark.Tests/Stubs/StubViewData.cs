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

    public class StubViewData<TModel> : StubViewData
    {
        public TModel Model { get; set; }
    }
}