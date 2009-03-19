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

namespace Spark.Parser
{
    public static class ParseActionExtensions
    {
        public static ParseAction<IList<TValue>> Rep<TValue>(this ParseAction<TValue> parse)
        {
            return Grammar.Rep(parse);
        }
        public static ParseAction<IList<TValue>> Rep1<TValue>(this ParseAction<TValue> parse)
        {
            return Grammar.Rep1(parse);
        }

        public static ParseAction<Chain<TValue1, TValue2>> And<TValue1, TValue2>(
            this ParseAction<TValue1> p1,
            ParseAction<TValue2> p2)
        {
            return Grammar.And(p1, p2);
        }
        public static ParseAction<TValue> Or<TValue>(
            this ParseAction<TValue> p1,
            ParseAction<TValue> p2)
        {
            return Grammar.Or(p1, p2);
        }

        public static ParseAction<TValue1> IfNext<TValue1, TValue2>(
            this ParseAction<TValue1> p1,
            ParseAction<TValue2> p2)
        {
            return Grammar.IfNext(p1, p2);
        }

        public static ParseAction<TValue1> NotNext<TValue1, TValue2>(
            this ParseAction<TValue1> p1,
            ParseAction<TValue2> p2)
        {
            return Grammar.NotNext(p1, p2);
        }
        public static ParseAction<TValue1> Unless<TValue1, TValue2>(
            this ParseAction<TValue1> p1,
            ParseAction<TValue2> p2)
        {
            return Grammar.Unless(p1, p2);
        }

        public static ParseAction<TValue2> Build<TValue1, TValue2>(
            this ParseAction<TValue1> parser,
            Func<TValue1, TValue2> builder)
        {
            return input =>
                       {
                           var result = parser(input);
                           if (result == null) return null;
                           return new ParseResult<TValue2>(result.Rest, builder(result.Value));
                       };
        }
        public static ParseAction<TValue> Opt<TValue>(ParseAction<TValue> parse)
        {
            return Grammar.Opt(parse);
        }

        public static ParseAction<TLeft> Left<TLeft, TDown>(this ParseAction<Chain<TLeft, TDown>> parse)
        {
            return input =>
            {
                var result = parse(input);
                if (result == null) return null;
                return new ParseResult<TLeft>(result.Rest, result.Value.Left);
            };
        }

        public static ParseAction<TDown> Down<TLeft, TDown>(this ParseAction<Chain<TLeft, TDown>> parse)
        {
            return input =>
            {
                var result = parse(input);
                if (result == null) return null;
                return new ParseResult<TDown>(result.Rest, result.Value.Down);
            };
        }

        public static ParseAction<TValue> Paint<TValue>(this ParseAction<TValue> parser)
        {
            return Grammar.Paint(parser);
        }

        public static ParseAction<TValue> Paint<TValue, TPaintValue>(this ParseAction<TValue> parser) where TValue : TPaintValue
        {
            return Grammar.Paint<TValue, TPaintValue>(parser);
        }

        public static ParseAction<TValue> Paint<TValue, TPaintValue>(this ParseAction<TValue> parser, TPaintValue value)
        {
            return Grammar.Paint(value, parser);
        }

        //public static ParseAction<TValue> TokenText<TValue>(this ParseAction<TValue> parser)
        //{ return parser.Paint(SparkTokenType.Text); }
        //public static ParseAction<TValue> TokenKeyword<TValue>(this ParseAction<TValue> parser)
        //{ return parser.Paint(SparkTokenType.Keyword); }
        //public static ParseAction<TValue> TokenIdentifier<TValue>(this ParseAction<TValue> parser)
        //{ return parser.Paint(SparkTokenType.Identifier); }
        //public static ParseAction<TValue> TokenString<TValue>(this ParseAction<TValue> parser)
        //{ return parser.Paint(SparkTokenType.String); }
        //public static ParseAction<TValue> TokenLiteral<TValue>(this ParseAction<TValue> parser)
        //{ return parser.Paint(SparkTokenType.Literal); }
        //public static ParseAction<TValue> TokenOperator<TValue>(this ParseAction<TValue> parser)
        //{ return parser.Paint(SparkTokenType.Operator); }
        //public static ParseAction<TValue> TokenDelimiter<TValue>(this ParseAction<TValue> parser)
        //{ return parser.Paint(SparkTokenType.Delimiter); }
        //public static ParseAction<TValue> TokenWhiteSpace<TValue>(this ParseAction<TValue> parser)
        //{ return parser.Paint(SparkTokenType.WhiteSpace); }
        //public static ParseAction<TValue> TokenLineComment<TValue>(this ParseAction<TValue> parser)
        //{ return parser.Paint(SparkTokenType.LineComment); }
        //public static ParseAction<TValue> TokenComment<TValue>(this ParseAction<TValue> parser)
        //{ return parser.Paint(SparkTokenType.Comment); }
    }

}
