using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Parser.Markup;

namespace Spark.Parser.Code
{
    public static class AttributeNodeExtensions
    {
        static CodeGrammar _grammar = new CodeGrammar();

        public static string AsCode(this AttributeNode node)
        {
            var position = new Position(new SourceContext(node.Value));
            var result = _grammar.Expression(position);
            return result.Value + result.Rest.Peek(result.Rest.PotentialLength());
        }
    }
}
