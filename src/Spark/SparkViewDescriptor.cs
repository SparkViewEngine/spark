using System.Collections.Generic;

namespace Spark
{
    public class SparkViewDescriptor
    {
        public SparkViewDescriptor()
        {
            Templates = new List<string>();
        }

        public string TargetNamespace { get; set; }
        public IList<string> Templates { get; set; }

        public SparkViewDescriptor SetTargetNamespace(string targetNamespace)
        {
            TargetNamespace = targetNamespace;
            return this;
        }

        public SparkViewDescriptor AddTemplate(string template)
        {
            Templates.Add(template);
            return this;
        }
    }
}
