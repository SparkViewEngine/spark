using System.Collections.Generic;
using System.Linq;

namespace Spark.Bindings
{
    public class BindingNode
    {

    }

    public class BindingLiteral : BindingNode
    {
        public BindingLiteral(IEnumerable<char> text)
        {
            Text = new string(text.ToArray());
        }

        public string Text { get; set; }
    }

    public class BindingNameReference : BindingNode
    {
        public BindingNameReference(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public bool AssumeStringValue { get; set; }
    }

    public class BindingPrefixReference : BindingNode
    {
        public BindingPrefixReference(string prefix)
        {
            Prefix = prefix;
        }

        public string Prefix { get; set; }
        public bool AssumeStringValue { get; set; }
    }

}