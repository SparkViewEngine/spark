using System.Collections.Generic;

namespace Spark
{
    public class SparkViewDescriptor
    {
        public SparkViewDescriptor()
        {
            Templates = new List<string>();
            Accessors = new List<Accessor>();
        }

        public string TargetNamespace { get; set; }
        public IList<string> Templates { get; set; }
        public IList<Accessor> Accessors { get; set; }

        public class Accessor
        {
            public string Property { get; set; }
            public string GetValue { get; set; }
        }

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

        public SparkViewDescriptor AddAccessor(string property, string getValue)
        {
            Accessors.Add(new Accessor { Property = property, GetValue = getValue });
            return this;
        }
    }
}
