using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark.FileSystem;

namespace Spark.Tests.Stubs
{
    public class StubViewFactory
    {
        public ISparkViewEngine Engine { get; set; }

        public void RenderView(StubViewContext viewContext)
        {
            var descriptor = new SparkViewDescriptor()
                                 {
                                     ControllerName = viewContext.ControllerName,
                                     ViewName = viewContext.ViewName,
                                     MasterName = viewContext.MasterName
                                 };
            var sparkView = Engine.CreateInstance(descriptor);
            ((StubSparkView)sparkView).ViewData = viewContext.Data;
            sparkView.RenderView(new StringWriter(viewContext.Output));
        }
    }
}