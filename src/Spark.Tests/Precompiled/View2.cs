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
        Templates = new[] { "Hello\\World.spark", "Shared\\Default.spark" })]
    public class View2 : ISparkView
    {
        public void RenderView(TextWriter writer)
        {
            writer.Write("<p>Hello world</p>");
        }

        public Guid GeneratedViewId
        {
            get { return new Guid("22222222123412341234123456123456"); }
        }
    }
}
