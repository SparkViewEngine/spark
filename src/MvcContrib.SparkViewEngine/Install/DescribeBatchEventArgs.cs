using System;
using Spark;

namespace MvcContrib.SparkViewEngine.Install
{
    public class DescribeBatchEventArgs : EventArgs
    {
        public SparkBatchDescriptor Batch { get; set; }
    }
}