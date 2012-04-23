// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Spark.Parser.Markup;

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

            var quotStringLiteral = Snip(Ch('\"').And(Rep(quotHunks)).And(Ch('\"')), hit => "\"" + js(hit.Left.Down) + "\"");


            var quotVerbatimPiece =
                Ch("\"\"").Or(ChNot('\"').Build(ch => new string(ch, 1)));

            var quotVerbatimLiteral = Snip(Ch("@\"").And(Rep(quotVerbatimPiece)).And(Ch('"')), hit => "@\"" + js(hit.Left.Down) + "\"");

            var aposHunks =
                Snip(Rep1(ChNot('\'', '\\', '\"')))
                .Or(Snip(escapeSequence))
                .Or(Swap(Ch('\"'), "\\\""));

            var aposStringLiteral = Snip(Swap(Ch('\''), "\"").And(Snip(Rep(aposHunks))).And(Swap(Ch('\''), "\"")));

            // @' " '' ' becomes @" "" ' "
            var aposVerbatimPiece =
                Swap(Ch("''"), "'")
                .Or(Swap(Ch('\"'), "\"\""))
                .Or(Snip(ChNot('\'')));

            var aposVerbatimLiteral = Snip(Swap(Ch("@'"), "@\"").And(Snip(Rep(aposVerbatimPiece))).And(Swap(Ch('\''), "\"")));

            var stringLiteral = TkStr(quotStringLiteral.Or(quotVerbatimLiteral).Or(aposStringLiteral).Or(aposVerbatimLiteral));

            _stringLiteral = stringLiteral;

            var SpecialCharCast = Snip(Ch("(char)'").And(ChNot('\'', '\\').Build(ch => ch.ToString()).Or(escapeSequence)).And(Ch('\'')),
                hit => "(char)'" + hit.Left.Down + "'");

            var oneLineComment = Snip(Ch("//").And(Rep(ChNot('\r', '\n'))), hit => "//" + bs(hit.Down));

            var multiLineComment = Snip(Ch("/*").And(Rep(Ch(c => true).Unless(Ch("*/")))).And(Ch("*/")),
                hit => "/*" + bs(hit.Left.Down) + "*/");

            // A Unicode character of the class Pc 
            var connectingCharacter = Ch(c => char.GetUnicodeCategory(c) == UnicodeCategory.ConnectorPunctuation);

            // A Unicode character of classes Mn or Mc 
            var combiningCharacter = Ch(c => char.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark || char.GetUnicodeCategory(c) == UnicodeCategory.SpacingCombiningMark);

            // A Unicode character of the class Cf 
            var formattingCharacter = Ch(c => char.GetUnicodeCategory(c) == UnicodeCategory.Format);

            var identifierStartCharacter = Ch(char.IsLetter).Or(Ch('_'));
            var identifierPartCharacter = Ch(char.IsLetterOrDigit).Or(connectingCharacter).Or(combiningCharacter).Or(formattingCharacter);
            var identifierOrKeyword = identifierStartCharacter.And(Rep(identifierPartCharacter))
                .Build(hit => hit.Left + new string(hit.Down.ToArray()));

            var keyword = Snip(Ch("abstract").Or(Ch("as")).Or(Ch("base")).Or(Ch("bool")).Or(Ch("break"))
                .Or(Ch("byte")).Or(Ch("case")).Or(Ch("catch")).Or(Ch("char")).Or(Ch("checked"))
                .Or(Ch("class")).Or(Ch("const")).Or(Ch("continue")).Or(Ch("decimal")).Or(Ch("default"))
                .Or(Ch("delegate")).Or(Ch("double")).Or(Ch("do")).Or(Ch("else")).Or(Ch("enum"))
                .Or(Ch("event")).Or(Ch("explicit")).Or(Ch("extern")).Or(Ch("false")).Or(Ch("finally"))
                .Or(Ch("fixed")).Or(Ch("float")).Or(Ch("foreach")).Or(Ch("for")).Or(Ch("goto"))
                .Or(Ch("if")).Or(Ch("implicit")).Or(Ch("int")).Or(Ch("in")).Or(Ch("interface"))
                .Or(Ch("internal")).Or(Ch("is")).Or(Ch("lock")).Or(Ch("long")).Or(Ch("namespace"))
                .Or(Ch("new")).Or(Ch("null")).Or(Ch("object")).Or(Ch("operator")).Or(Ch("out"))
                .Or(Ch("override")).Or(Ch("params")).Or(Ch("private")).Or(Ch("protected")).Or(Ch("public"))
                .Or(Ch("readonly")).Or(Ch("ref")).Or(Ch("return")).Or(Ch("sbyte")).Or(Ch("sealed"))
                .Or(Ch("short")).Or(Ch("sizeof")).Or(Ch("stackalloc")).Or(Ch("static")).Or(Ch("string"))
                .Or(Ch("struct")).Or(Ch("switch")).Or(Ch("this")).Or(Ch("throw")).Or(Ch("true"))
                .Or(Ch("try")).Or(Ch("typeof")).Or(Ch("uint")).Or(Ch("ulong")).Or(Ch("unchecked"))
                .Or(Ch("unsafe")).Or(Ch("ushort")).Or(Ch("using")).Or(Ch("virtual")).Or(Ch("void"))
                .Or(Ch("volatile")).Or(Ch("while"))).NotNext(identifierPartCharacter);

            var availableIdentifier = Snip(identifierOrKeyword.Unless(keyword))
                .Or(Swap(Ch("class"), "@class"));

            var identifier = availableIdentifier
                .Or(Snip(Ch('@').And(identifierOrKeyword), hit => "@" + hit.Down));

            // parsing late bound properties #a.b.c into Eval("a.b.c")

            var dotProperty = Snip(Ch('.').And(identifierOrKeyword),
                                   hit => hit.Left + hit.Down);

            var formatAppendage = Swap(Opt(Rep(Ch(' ', '\t')))
                                           .And(Ch('\"').And(Rep1(ChNot('\"'))).And(Ch('\"'))
                                                    .Or(Ch('\'').And(Rep1(ChNot('\''))).And(Ch('\'')))),
                                       hit => ", \"{0:" + new string(hit.Down.Left.Down.ToArray()) + "}\"");

            var lateBound = Ch('#')
                .And(Snip(identifierOrKeyword))
                .And(Snip(Rep(dotProperty)))
                .And(Opt(formatAppendage))
                .Build(hit => (IList<Snippet>) new Snippets("Eval(\"")
                                                   .Concat(hit.Left.Left.Down)
                                                   .Concat(hit.Left.Down)
                                                   .Concat(new Snippets("\""))
                                                   .Concat(hit.Down ?? new Snippet[0])
                                                   .Concat(new Snippets(")"))
                                                   .ToList());


            var codeStretch = Snip(Rep1(
                Swap(Ch("[["), "<")
                .Or(Swap(Ch("]]"), ">"))
                .Or(lateBound)
                .Or(Snip(ChNot('\"', '\'', '{', '}', '(', ')')))
                .Unless(identifier.Or(keyword).Or(SpecialCharCast))
                .Unless(Ch("%>").Or(Ch("@\"")).Or(Ch("@'")).Or(Ch("//")).Or(Ch("/*")))));

            Func<ParseAction<string>, ParseAction<IList<Snippet>>> limitedCodeStretch = limit => Snip(Rep1(
                Swap(Ch("[["), "<")
                .Or(Swap(Ch("]]"), ">"))
                .Or(lateBound)
                .Or(Snip(ChNot('\"', '\'', '{', '}', '(', ')')))
                .Unless(identifier.Or(keyword).Or(SpecialCharCast))
                .Unless(limit.Or(Ch("@\"")).Or(Ch("@'")).Or(Ch("//")).Or(Ch("/*")))));


            // braced ::= '{' + terms + '}'
            var braced = Snip(Snip(Ch('{')).And((ParseAction<IList<Snippet>>)FnTerms).And(Snip(Ch('}'))));

            // parens ::= '(' + terms + ')'
            var parens = Snip(Snip(Ch('(')).And((ParseAction<IList<Snippet>>)FnTerms).And(Snip(Ch(')')))).Unless(SpecialCharCast);

            // ExpressionTerms ::= (dquot | aquot | braced | codeStretch | specialCharCast)*            
            ExpressionTerms = Snip(Rep1(
                stringLiteral
                .Or(braced)
                .Or(parens)
                .Or(codeStretch)
                .Or(identifier)
                .Or(keyword)
                .Or(SpecialCharCast)
                .Or(oneLineComment)
                .Or(multiLineComment)
                )).Or(EmptySnip());

            LimitedExpressionTerms = limit=>Snip(Rep1(
                stringLiteral
                .Or(braced)
                .Or(parens)
                .Or(limitedCodeStretch(limit))
                .Or(identifier)
                .Or(keyword)
                .Or(SpecialCharCast)
                .Or(oneLineComment)
                .Or(multiLineComment)
                )).Or(EmptySnip());

            Expression = ExpressionTerms.Build(hit => new Snippets(hit));


            LimitedExpression = limit=>LimitedExpressionTerms(limit).Build(hit => new Snippets(hit));


            var statementPiece =
                Swap(Ch("[["), "<")
                .Or(Swap(Ch("]]"), ">"))
                .Or(lateBound)
                .Or(Snip(ChNot('\"', '\'')))
                .Unless(SpecialCharCast)
                .Unless(Ch("@\"").Or(Ch("@'")).Or(Ch("//")).Or(Ch("/*")));

            var statement1Stretch = Snip(Rep1(statementPiece.Unless(Ch('\r', '\n'))));

            var statement2Stretch = Snip(Rep1(statementPiece.Unless(Ch("%>"))));

            // Statement1 ::= (dquot | aquot | statement1Stretch | specialCharCast)*
            Statement1 = Snip(Rep(
                stringLiteral
                .Or(statement1Stretch)
                .Or(SpecialCharCast)
                .Or(oneLineComment)
                .Or(multiLineComment)));


            // Statement2 ::= (dquot | aquot | statement2Stretch | specialCharCast)*
            Statement2 = Snip(Rep(
                stringLiteral
                .Or(statement2Stretch)
                .Or(SpecialCharCast)
                .Or(oneLineComment)
                .Or(multiLineComment)));
        }



        static ParseAction<IList<Snippet>> Snip(ParseAction<Chain<Chain<IList<Snippet>, IList<Snippet>>, IList<Snippet>>> parser)
        {
            return Snip(parser, hit => new[] { hit.Left.Left, hit.Left.Down, hit.Down });
        }

        static ParseAction<IList<Snippet>> Snip<TValue>(ParseAction<TValue> parser, Func<TValue, IList<IList<Snippet>>> combiner)
        {
            return position =>
            {
                var result = parser(position);
                if (result == null) return null;
                return new ParseResult<IList<Snippet>>(result.Rest, combiner(result.Value).SelectMany(s => s).ToArray());
            };
        }

        static ParseAction<IList<Snippet>> Snip<TValue>(ParseAction<TValue> parser, Func<TValue, string> builder)
        {
            return position =>
            {
                var result = parser(position);
                if (result == null) return null;
                var snippet = new Snippet { Value = builder(result.Value), Begin = position, End = result.Rest };
                return new ParseResult<IList<Snippet>>(result.Rest, new[] { snippet });
            };
        }

        static ParseAction<IList<Snippet>> Snip(ParseAction<char> parser)
        {
            return position =>
            {
                var result = parser(position);
                if (result == null) return null;
                var snippet = new Snippet { Value = new string(result.Value, 1), Begin = position, End = result.Rest };
                return new ParseResult<IList<Snippet>>(result.Rest, new[] { snippet });
            };
        }

        static ParseAction<IList<Snippet>> Snip(ParseAction<IList<char>> parser)
        {
            return position =>
            {
                var result = parser(position);
                if (result == null) return null;
                var snippet = new Snippet { Value = new string(result.Value.ToArray()), Begin = position, End = result.Rest };
                return new ParseResult<IList<Snippet>>(result.Rest, new[] { snippet });
            };
        }

        static ParseAction<IList<Snippet>> Snip(ParseAction<string> parser)
        {
            return position =>
            {
                var result = parser(position);
                if (result == null) return null;
                var snippet = new Snippet { Value = new string(result.Value.ToArray()), Begin = position, End = result.Rest };
                return new ParseResult<IList<Snippet>>(result.Rest, new[] { snippet });
            };
        }
        static ParseAction<IList<Snippet>> Snip(ParseAction<IList<string>> parser)
        {
            return position =>
            {
                var result = parser(position);
                if (result == null) return null;
                var snippet = new Snippet { Value = string.Concat(result.Value.ToArray()), Begin = position, End = result.Rest };
                return new ParseResult<IList<Snippet>>(result.Rest, new[] { snippet });
            };
        }
        static ParseAction<IList<Snippet>> Snip(ParseAction<IList<Snippet>> parser)
        {
            return parser;
        }
        static ParseAction<IList<Snippet>> Snip(ParseAction<IList<IList<Snippet>>> parser)
        {
            return position =>
            {
                var result = parser(position);
                if (result == null) return null;
                return new ParseResult<IList<Snippet>>(result.Rest, result.Value.SelectMany(s => s).ToArray());
            };
        }
        static ParseAction<IList<Snippet>> EmptySnip()
        {
            return position => new ParseResult<IList<Snippet>>(position, new[] { new Snippet { Value = "", Begin = position, End = position } });
        }

        static ParseAction<IList<Snippet>> Swap<TValue>(ParseAction<TValue> parser, string replacement)
        {
            return position =>
            {
                var result = parser(position);
                if (result == null) return null;
                var snippet = new Snippet { Value = replacement };
                return new ParseResult<IList<Snippet>>(result.Rest, new[] { snippet });
            };
        }
        static ParseAction<IList<Snippet>> Swap<TValue>(ParseAction<TValue> parser, Func<TValue, string> replacement)
        {
            return position =>
            {
                var result = parser(position);
                if (result == null) return null;
                var snippet = new Snippet { Value = replacement(result.Value) };
                return new ParseResult<IList<Snippet>>(result.Rest, new[] { snippet });
            };
        }

        public ParseAction<IList<Snippet>> ExpressionTerms;
        public ParseAction<Snippets> Expression;

        public Func<ParseAction<string>, ParseAction<IList<Snippet>>> LimitedExpressionTerms;
        public Func<ParseAction<string>, ParseAction<Snippets>> LimitedExpression;

        public ParseAction<IList<Snippet>> Statement1;
        public ParseAction<IList<Snippet>> Statement2;

        public ParseAction<IList<Snippet>> _stringLiteral;

        ParseResult<IList<Snippet>> FnTerms(Position position)
        {
            return ExpressionTerms(position);
        }

        protected static ParseAction<TValue> TkTagDelim<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.HtmlTagDelimiter, parser); }
        protected static ParseAction<TValue> TkEleNam<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.HtmlElementName, parser); }
        protected static ParseAction<TValue> TkAttNam<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.HtmlAttributeName, parser); }
        protected static ParseAction<TValue> TkAttDelim<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.HtmlOperator, parser); }
        protected static ParseAction<TValue> TkAttQuo<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.HtmlAttributeValue, parser); }
        protected static ParseAction<TValue> TkAttVal<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.HtmlAttributeValue, parser); }
        protected static ParseAction<TValue> TkEntity<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.HtmlEntity, parser); }
        protected static ParseAction<TValue> TkComm<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.HtmlComment, parser); }
        protected static ParseAction<TValue> TkCDATA<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.PlainText, parser); }

        protected static ParseAction<TValue> TkAspxCode<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.HtmlServerSideScript, parser); }
        protected static ParseAction<TValue> TkCode<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.SparkDelimiter, parser); }
        protected static ParseAction<TValue> TkStr<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.String, parser); }
    }
}
