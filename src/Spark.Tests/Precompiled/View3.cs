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
        Templates = new[] { "Hello\\Sailor.spark", "Shared\\Default.spark" })]
    public class View3 
    {
        public void RenderView(TextWriter writer)
        {
            writer.Write("<p>Hello world</p>");
        }

        public Guid GeneratedViewId
        {
            get { return new Guid("33333333123412341234123456123456"); }
        }
    }
}
