using System.Collections.Generic;

namespace Spark
{
    public class SparkViewDescriptor
    {
        public SparkViewDescriptor()
        {
            Templates = new List<string>();
        }
        //public string ControllerName { get; set; }
        //public string ViewName { get; set; }
        //public string MasterName { get; set; }

        public IList<string> Templates { get; set; }
    }
}