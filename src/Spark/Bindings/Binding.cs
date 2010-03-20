using System.Collections.Generic;

namespace Spark.Bindings
{
    public class Binding
    {
        public string ElementName { get; set; }
        public IEnumerable<BindingPhrase> Phrases { get; set; }
    }
}
