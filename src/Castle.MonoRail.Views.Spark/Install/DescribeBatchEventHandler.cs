using System;
using Castle.MonoRail.Views.Spark.Install;
using Spark;

namespace Castle.MonoRail.Views.Spark.Install
{
    public delegate void DescribeBatchEventHandler(object sender, DescribeBatchEventArgs e);

    public class DescribeBatchEventArgs : EventArgs
    {
        public SparkBatchDescriptor Batch { get; set; }
    }

}