using System.Collections.Generic;

namespace Spark
{
    public class SparkViewDescriptor
    {
        public SparkViewDescriptor()
        {
            Templates = new List<string>();
        }

        public IList<string> Templates { get; set; }
    }
}