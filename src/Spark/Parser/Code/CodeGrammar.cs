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

            var SpecialCharCast = Ch("(char)'").And(ChNot('\'', '\\').Build(ch => ch.ToString()).Or(escapeSequence)).And(Ch('\''))
                .Build(hit => "(char)'" + hit.Left.Down + "'");

            var codeStretch = Rep1(
                Ch("[[").Build(ch => '<')
                .Or(Ch("]]").Build(ch => '>'))
                .Or(ChNot('\"', '\'', '{', '}'))
                .Unless(Ch("%>").Or(SpecialCharCast)))
                .Build(bs);

            // braced ::= '{' + terms + '}'
            var braced = Ch('{').And((ParseAction<IList<string>>)FnTerms).And(Ch('}'))
                .Build(hit => "{" + js(hit.Left.Down) + "}");

            // ExpressionTerms ::= (dquot | aquot | braced | codeStretch | specialCharCast)*
            ExpressionTerms = Rep(quotStringLiteral.Or(aposStringLiteral).Or(braced).Or(codeStretch).Or(SpecialCharCast));

            Expression = ExpressionTerms.Build(hit => string.Concat(hit.ToArray()));


            // similar, but not identical
            var statement1Stretch = Rep1(
                Ch("[[").Build(ch => '<')
                .Or(Ch("]]").Build(ch => '>'))
                .Or(ChNot('\"', '\''))
                .Unless(Ch('\r', '\n'))
                .Unless(SpecialCharCast))
                .Build(bs);

            var statement2Stretch = Rep1(
                Ch("[[").Build(ch => '<')
                .Or(Ch("]]").Build(ch => '>'))
                .Or(ChNot('\"', '\''))
                .Unless(Ch("%>"))
                .Unless(SpecialCharCast))
                .Build(bs);

            // Statement1 ::= (dquot | aquot | statement1Stretch | specialCharCast)*
            Statement1 = Rep(quotStringLiteral.Or(aposStringLiteral).Or(statement1Stretch).Or(SpecialCharCast))
                .Build(hit => string.Concat(hit.ToArray()));

            // Statement2 ::= (dquot | aquot | statement2Stretch | specialCharCast)*
            Statement2 = Rep(quotStringLiteral.Or(aposStringLiteral).Or(statement2Stretch).Or(SpecialCharCast))
                .Build(hit => string.Concat(hit.ToArray()));

        }

        public ParseAction<IList<string>> ExpressionTerms;
        public ParseAction<string> Expression;

        public ParseAction<string> Statement1;
        public ParseAction<string> Statement2;

        ParseResult<IList<string>> FnTerms(Position position)
        {
            return ExpressionTerms(position);
        }
    }
}
