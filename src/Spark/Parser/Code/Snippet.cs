using System.Collections.Generic;
using System.Linq;

namespace Spark.Parser.Code
{
    public class Snippet
    {
        public string Value { get; set; }
        public Position Begin { get; set; }
        public Position End { get; set; }
    }

    public class SnippetCollection : List<Snippet>
    {
        public SnippetCollection()
        {
        }

        public SnippetCollection(int capacity)
            : base(capacity)
        {
        }

        public SnippetCollection(IEnumerable<Snippet> collection)
            : base(collection)
        {
        }

        public override string ToString()
        {
            return string.Concat(this.Select(s => s.Value).ToArray());
        }

        public static implicit operator string(SnippetCollection c)
        {
            return c.ToString();
        }
    }
}

