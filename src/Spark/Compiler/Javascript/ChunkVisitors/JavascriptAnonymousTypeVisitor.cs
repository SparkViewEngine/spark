using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler.ChunkVisitors;
using Spark.Parser;
using Spark.Parser.Code;

namespace Spark.Compiler.Javascript.ChunkVisitors
{
    public class JavascriptAnonymousTypeVisitor : CodeProcessingChunkVisitor
    {
        private static readonly JavascriptAnonymousTypeGrammar _grammar = new JavascriptAnonymousTypeGrammar();

        public override Snippets Process(Chunk chunk, Snippets code)
        {
            if (code == null)
                return null;

            var result = _grammar.ReformatCode(new Position(new SourceContext(code.ToString())));

            if (result == null)
                return code;

            if (result.Rest.PotentialLength() == 0)
                return result.Value;

            return result.Value + result.Rest.Peek(result.Rest.PotentialLength());
        }
    }

    public class JavascriptAnonymousTypeGrammar : CharGrammar
    {
        public JavascriptAnonymousTypeGrammar()
        {
            var ws = Rep(Ch(' ', '\t', '\r', '\n'));

            var propName = Ch(char.IsLetter).Or(Ch('_')).And(Rep(Ch(char.IsLetterOrDigit).Or(Ch('_'))));

            var doubleString = Ch('\"').And(Rep(ChNot('\"'))).And(Ch('\"'));
            var singleString = Ch('\'').And(Rep(ChNot('\''))).And(Ch('\''));

            ParseAction<string> valuePart = ValuePart;

            var valueFiller = Str(Rep1(ChNot('}', ',').Unless(valuePart)));

            var propValue = Rep(valueFiller.Or(valuePart));

            var term = ws.And(Str(propName)).And(ws).And(Ch('=')).And(ws).And(propValue)
                .Build(hit => hit.Left.Left.Left.Left.Down + ":" + string.Concat(hit.Down.ToArray()));

            var termDelimiter = ws.And(Ch(',')).And(ws);

            var terms = term.And(Rep(termDelimiter.And(term)))
                .Build(hit => new[] { hit.Left }.Concat(hit.Down.Select(x => x.Down)));

            var anonymousType = Ch("new").And(ws).And(Ch('{')).And(terms).And(Ch('}'))
                .Build(hit => "{" + string.Join(",", hit.Left.Down.ToArray()) + "}");

            _valuePart = Str(doubleString.Or(singleString)).Or(anonymousType);

            var codeFiller = Rep1(Ch(ch => true).Unless(anonymousType));

            ReformatCode = Rep(Str(codeFiller).Or(anonymousType))
                .Build(hit => string.Concat(hit.ToArray()));

            test_ws = Test(ws);
            test_doubleString = Test(doubleString);
            test_singleString = Test(singleString);
            test_propName = Test(propName);
            test_propValue = Test(propValue);
            test_term = Test(term);
            test_terms = Test(terms);
            test_anonymousType = anonymousType;
        }

        private static ParseAction<string> Str<TValue>(ParseAction<TValue> parser)
        {
            return pos =>
            {
                var result = parser(pos);
                if (result == null) return null;
                return new ParseResult<string>(result.Rest, pos.Peek(result.Rest.Offset - pos.Offset));
            };
        }

        private readonly ParseAction<string> _valuePart;

        ParseResult<string> ValuePart(Position pos)
        {
            return _valuePart(pos);
        }

        /// <summary>
        /// Method to turn any parse action result into the string value of the matched position
        /// </summary>
        private static ParseAction<string> Test<TValue>(ParseAction<TValue> parser)
        {
            return Str(parser);
        }

        public ParseAction<string> ReformatCode { get; set; }

        public ParseAction<string> test_ws;
        public ParseAction<string> test_doubleString;
        public ParseAction<string> test_singleString;
        public ParseAction<string> test_propName;
        public ParseAction<string> test_propValue;
        public ParseAction<string> test_term;
        public ParseAction<string> test_terms;
        public ParseAction<string> test_anonymousType;
    }
}
