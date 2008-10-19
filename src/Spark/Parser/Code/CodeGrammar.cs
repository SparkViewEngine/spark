// Copyright 2008 Louis DeJardin - http://whereslou.com
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

            var quotStringLiteral = Ch('\"').And(Rep(quotHunks)).And(Ch('\"'))
                .Build(hit => "\"" + js(hit.Left.Down) + "\"");


            var quotVerbatimPiece =
                Ch("\"\"").Or(ChNot('\"').Build(ch => new string(ch, 1)));

            var quotVerbatimLiteral = Ch("@\"").And(Rep(quotVerbatimPiece)).And(Ch('"'))
                .Build(hit => "@\"" + js(hit.Left.Down) + "\"");


            var aposHunks =
                Rep1(ChNot('\'', '\\', '\"')).Build(bs)
                .Or(escapeSequence)
                .Or(Ch('\"').Build(hit => "\\\""));

            var aposStringLiteral = Ch('\'').And(Rep(aposHunks)).And(Ch('\''))
                .Build(hit => "\"" + js(hit.Left.Down) + "\"");

            // @' " '' ' becomes @" "" ' "
            var aposVerbatimPiece =
                Ch("''").Build(ch => "'")
                .Or(Ch("\"").Build(ch => "\"\""))
                .Or(ChNot('\'').Build(ch => new string(ch, 1)));

            var aposVerbatimLiteral = Ch("@'").And(Rep(aposVerbatimPiece)).And(Ch('\''))
                .Build(hit => "@\"" + js(hit.Left.Down) + "\"");

            var stringLiteral = TkStr(quotStringLiteral.Or(quotVerbatimLiteral).Or(aposStringLiteral).Or(aposVerbatimLiteral));

            var SpecialCharCast = Ch("(char)'").And(ChNot('\'', '\\').Build(ch => ch.ToString()).Or(escapeSequence)).And(Ch('\''))
                .Build(hit => "(char)'" + hit.Left.Down + "'");

            var oneLineComment = TkComm(Ch("//").And(Rep(ChNot('\r', '\n'))))
                .Build(hit=>"//" + bs(hit.Down));

            var multiLineComment = TkComm(Ch("/*").And(Rep(Ch(c=>true).Unless(Ch("*/")))).And(Ch("*/")))
                .Build(hit=>"/*" + bs(hit.Left.Down) + "*/");

            // A Unicode character of the class Pc 
            var connectingCharacter = Ch(c => char.GetUnicodeCategory(c) == UnicodeCategory.ConnectorPunctuation);

            // A Unicode character of classes Mn or Mc 
            var combiningCharacter = Ch(c => char.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark || char.GetUnicodeCategory(c) == UnicodeCategory.SpacingCombiningMark);

            // A Unicode character of the class Cf 
            var formattingCharacter = Ch(c=>char.GetUnicodeCategory(c) == UnicodeCategory.Format);

            var identifierStartCharacter = Ch(char.IsLetter).Or(Ch('_'));
            var identifierPartCharacter = Ch(char.IsLetterOrDigit).Or(connectingCharacter).Or(combiningCharacter).Or(formattingCharacter);
            var identifierOrKeyword = identifierStartCharacter.And(Rep(identifierPartCharacter))
                .Build(hit=>hit.Left + new string(hit.Down.ToArray()));

            var keyword = TkKword(Ch("abstract").Or(Ch("as")).Or(Ch("base")).Or(Ch("bool")).Or(Ch("break"))
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

            var availableIdentifier = identifierOrKeyword.Unless(keyword)
                .Or(Ch("class").Build(hit => "@class"));

            var identifier = TkCode(availableIdentifier
                .Or(Ch('@').And(identifierOrKeyword).Build(hit=>"@"+hit.Down)));
            
            var codeStretch = TkCode(Rep1(
                Ch("[[").Build(ch => '<')
                .Or(Ch("]]").Build(ch => '>'))
                .Or(ChNot('\"', '\'', '{', '}'))
                .Unless(identifier.Or(keyword))
                .Unless(Ch("%>").Or(Ch("@\"")).Or(Ch("@'")).Or(Ch("//")).Or(Ch("/*")).Or(SpecialCharCast))))
                .Build(bs);

            // braced ::= '{' + terms + '}'
            var braced = TkDelim(Ch('{')).And((ParseAction<IList<string>>)FnTerms).And(TkDelim(Ch('}')))
                .Build(hit => "{" + js(hit.Left.Down) + "}");

            // ExpressionTerms ::= (dquot | aquot | braced | codeStretch | specialCharCast)*
            ExpressionTerms = Rep(
                stringLiteral
                .Or(braced)
                .Or(codeStretch)
                .Or(identifier)
                .Or(keyword)
                .Or(SpecialCharCast)
                .Or(oneLineComment)
                .Or(multiLineComment));

            Expression = ExpressionTerms.Build(hit => string.Concat(hit.ToArray()));


            var statementPiece =
                Ch("[[").Build(ch => '<')
                .Or(Ch("]]").Build(ch => '>'))
                .Or(ChNot('\"', '\''))
                .Unless(SpecialCharCast.Or(Ch("@\"")).Or(Ch("@'")).Or(Ch("//")).Or(Ch("/*")));

            var statement1Stretch = TkCode(Rep1(statementPiece.Unless(Ch('\r', '\n'))))
                .Build(bs);

            var statement2Stretch = TkCode(Rep1(statementPiece.Unless(Ch("%>"))))
                .Build(bs);

            // Statement1 ::= (dquot | aquot | statement1Stretch | specialCharCast)*
            Statement1 = Rep(
                stringLiteral
                .Or(statement1Stretch)
                .Or(SpecialCharCast)
                .Or(oneLineComment)
                .Or(multiLineComment))
                .Build(hit => string.Concat(hit.ToArray()));

            // Statement2 ::= (dquot | aquot | statement2Stretch | specialCharCast)*
            Statement2 = Rep(
                stringLiteral
                .Or(statement2Stretch)
                .Or(SpecialCharCast)
                .Or(oneLineComment)
                .Or(multiLineComment))
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

        protected static ParseAction<TValue> TkAttNam<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.AttributeName, parser); }
        protected static ParseAction<TValue> TkAttQuo<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.AttributeQuotes, parser); }
        protected static ParseAction<TValue> TkAttVal<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.AttributeValue, parser); }
        protected static ParseAction<TValue> TkCDATA<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.CDATASection, parser); }
        protected static ParseAction<TValue> TkComm<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.Comment, parser); }
        protected static ParseAction<TValue> TkDelim<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.Delimiter, parser); }
        protected static ParseAction<TValue> TkKword<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.Keyword, parser); }
        protected static ParseAction<TValue> TkCode<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.Code, parser); }
        protected static ParseAction<TValue> TkEleNam<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.ElementName, parser); }
        protected static ParseAction<TValue> TkText<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.Text, parser); }
        protected static ParseAction<TValue> TkPI<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.ProcessingInstruction, parser); }
        protected static ParseAction<TValue> TkStr<TValue>(ParseAction<TValue> parser)
        { return Paint(SparkTokenType.String, parser); }

    }
}
