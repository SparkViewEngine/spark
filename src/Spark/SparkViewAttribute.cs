using System;

namespace Spark
{
    public class SparkViewAttribute : Attribute
    {
        public string TargetNamespace { get; set; }
        public string[] Templates { get; set; }

        public SparkViewDescriptor BuildDescriptor()
        {
            return new SparkViewDescriptor { TargetNamespace = TargetNamespace, Templates = Templates };
        }
    }
}