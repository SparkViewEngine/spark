using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark.Tests.Stubs;

namespace Spark.Tests.Precompiled
{
    [SparkView(
        TargetNamespace = "Spark.Tests.Precompiled", 
        Templates = new[] { "Foo\\Bar.spark", "Shared\\Quux.spark" })]
    public class View1 : StubSparkView
    {
        public override void RenderView(TextWriter writer)
        {
            writer.Write("<p>Hello world</p>");
        }

        public override Guid GeneratedViewId
        {
            get { return new Guid("11111111123412341234123456123456"); }
        }
    }
}
