using System.Collections.Generic;

namespace Spark.Bindings
{
    public class Binding
    {
        public string ElementName { get; set; }
        public IList<BindingNode> Nodes {get;set;}
    }
}
