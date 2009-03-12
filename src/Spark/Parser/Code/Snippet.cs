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

    public class Snippets : List<Snippet>
    {
        public Snippets()
        {
        }

        public Snippets(int capacity)
            : base(capacity)
        {
        }

        public Snippets(IEnumerable<Snippet> collection)
            : base(collection)
        {
        }

        public Snippets(Snippets collection)
            : base((Snippets)collection)
        {
        }

        public Snippets(string value)
            : base(new[] { new Snippet { Value = value } })
        {
        }

        public override string ToString()
        {
            return string.Concat(this.Select(s => s.Value).ToArray());
        }

        //public override bool Equals(object obj)
        //{
        //    return Equals(ToString(), (obj ?? "").ToString());
        //}
        //public override int GetHashCode()
        //{
        //    return ToString().GetHashCode();
        //}

        public static implicit operator string(Snippets c)
        {
            return c == null ? null : c.ToString();
        }

        public static implicit operator Snippets(string value)
        {
            return new Snippets(value);
        }

        public static bool IsNullOrEmpty(Snippets value)
        {
            if (value == null ||
                value.Count == 0 ||
                value.All(s => string.IsNullOrEmpty(s.Value)))
            {
                return true;
            }
            return false;
        }
    }
}

