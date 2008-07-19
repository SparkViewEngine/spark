using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark.Parser.Code
{
    public class CodeGrammar : CharGrammar
    {
        public CodeGrammar()
        {
            Func<IList<char>, string> bs = hit => new string(hit.ToArray());
            Func<IList<string>, string> js = hit => string.Concat(hit.ToArray());

            var escapeSequence = Ch('\\').And(Ch(c => true)).Build(hit => "\\" + hit.Down);


            var quotHunks =
                Rep1(ChNot('\"', '\\')).Build(bs)
                .Or(escapeSequence);

            var quotStringLiteral = Ch('\"').And(Rep(quotHunks)).And(Ch('\"'))
                .Build(hit => "\"" + js(hit.Left.Down) + "\"");

            var aposHunks =
                Rep1(ChNot('\'', '\\', '\"')).Build(bs)
                .Or(escapeSequence)
                .Or(Ch('\"').Build(hit => "\\\""));

            var aposStringLiteral = Ch('\'').And(Rep(aposHunks)).And(Ch('\''))
                .Build(hit => "\"" + js(hit.Left.Down) + "\"");
            
            var SpecialCharCast = Ch("(char)'").And(ChNot('\'', '\\').Build(ch=>ch.ToString()).Or(escapeSequence)).And(Ch('\''))
                .Build(hit => "(char)'" + hit.Left.Down + "'");

            var codeStretch = Rep1(
                Ch("[[").Build(ch=>'<')
                .Or(Ch("]]").Build(ch=>'>'))
                .Or(ChNot('\"', '\'', '{', '}'))
                .Unless(Ch("%>"))
                .Unless(SpecialCharCast))
                .Build(bs);

            // braced ::= '{' + terms + '}'
            var braced = Ch('{').And((ParseAction<IList<string>>)FnTerms).And(Ch('}'))
                .Build(hit => "{" + js(hit.Left.Down) + "}");

            // terms ::= (dquot | aquot | braced | codeStretch)*
            Terms = Rep(quotStringLiteral.Or(aposStringLiteral).Or(braced).Or(codeStretch).Or(SpecialCharCast));
            
            Expression = Terms.Build(hit => string.Concat(hit.ToArray()));
        }

        public ParseAction<IList<string>> Terms;
        public ParseAction<string> Expression;

        ParseResult<IList<string>> FnTerms(Position position)
        {
            return Terms(position);
        }
    }
}
