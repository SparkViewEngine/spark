using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spark.Tests
{
    public static class SparkViewExtensions
    {
        public static string RenderView(this ISparkView view)
        {
            var writer = new StringWriter();
            view.RenderView(writer);
            return writer.ToString();
        }

    }
}
