//-------------------------------------------------------------------------
// <copyright file="ParseActionExtensions.cs">
// Copyright 2008-2010 Louis DeJardin - http://whereslou.com
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
// </copyright>
// <author>Louis DeJardin</author>
// <author>John Gietzen</author>
//-------------------------------------------------------------------------

namespace Spark.Parser
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Contains extension methods to give the ParseAction class a fluent syntax.
    /// </summary>
    public static class ParseActionExtensions
    {
        /// <summary>
        /// Repeats a match zero or more times.
        /// </summary>
        /// <typeparam name="TValue">The type of the match.</typeparam>
        /// <param name="parse">The match predicate to be repeated.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<IList<TValue>> Rep<TValue>(this ParseAction<TValue> parse)
        {
            return Grammar.Rep(parse);
        }

        /// <summary>
        /// Repeats a match one or more times.
        /// </summary>
        /// <typeparam name="TValue">The type of the match.</typeparam>
        /// <param name="parse">The match predicate to be repeated.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        /// <remarks>
        /// If the match cannot be performed at least once, the entire match fails.
        /// </remarks>
        public static ParseAction<IList<TValue>> Rep1<TValue>(this ParseAction<TValue> parse)
        {
            return Grammar.Rep1(parse);
        }

        /// <summary>
        /// Indicates a conjunction (logical and) between two matches.
        /// </summary>
        /// <typeparam name="TValue1">The type of the first match.</typeparam>
        /// <typeparam name="TValue2">The type of the second match.</typeparam>
        /// <param name="p1">The first requirement in the conjunction.</param>
        /// <param name="p2">The second requirement in the conjunction.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        /// <remarks>
        /// When a match is found, the two results are chained together.
        /// The first match corresponds to Chain.Left, and the second to Chain.Down.
        /// </remarks>
        public static ParseAction<Chain<TValue1, TValue2>> And<TValue1, TValue2>(
            this ParseAction<TValue1> p1,
            ParseAction<TValue2> p2)
        {
            return Grammar.And(p1, p2);
        }

        /// <summary>
        /// Indicates a disjunction (logical or) between two matches.
        /// </summary>
        /// <typeparam name="TValue">The type of the matches.</typeparam>
        /// <param name="p1">The first option in the disjunction.</param>
        /// <param name="p2">The second option in the disjunction.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<TValue> Or<TValue>(
            this ParseAction<TValue> p1,
            ParseAction<TValue> p2)
        {
            return Grammar.Or(p1, p2);
        }

        /// <summary>
        /// Uses the first match, as long as the second match succeeds immediately after the first.
        /// </summary>
        /// <typeparam name="TValue">The type of the first match.</typeparam>
        /// <typeparam name="TValue2">The type of the second match.</typeparam>
        /// <param name="parse">The match predicate to be used.</param>
        /// <param name="cond">The match predicate that must match immediately after <paramref name="parse"/>.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<TValue> IfNext<TValue, TValue2>(
            this ParseAction<TValue> parse,
            ParseAction<TValue2> cond)
        {
            return Grammar.IfNext(parse, cond);
        }

        /// <summary>
        /// Uses the first match, as long as the second match does not succeed immediately after the first.
        /// </summary>
        /// <typeparam name="TValue">The type of the first match.</typeparam>
        /// <typeparam name="TValue2">The type of the second match.</typeparam>
        /// <param name="parse">The match predicate to be used.</param>
        /// <param name="cond">The match predicate that must not match immediately after <paramref name="parse"/>.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<TValue> NotNext<TValue, TValue2>(
            this ParseAction<TValue> parse,
            ParseAction<TValue2> cond)
        {
            return Grammar.NotNext(parse, cond);
        }

        /// <summary>
        /// Uses the first match, unless the second match is possible at the same location.
        /// </summary>
        /// <typeparam name="TValue1">The type of the first match.</typeparam>
        /// <typeparam name="TValue2">The type of the second match.</typeparam>
        /// <param name="p1">The first match predicate.</param>
        /// <param name="p2">The second match predicate.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<TValue1> Unless<TValue1, TValue2>(
            this ParseAction<TValue1> p1,
            ParseAction<TValue2> p2)
        {
            return Grammar.Unless(p1, p2);
        }

        /// <summary>
        /// Builds a new parse result, based on the return value of the current match and a transformation function.
        /// </summary>
        /// <typeparam name="TValue1">The type of the match.</typeparam>
        /// <typeparam name="TValue2">The type returned from the transformation function.</typeparam>
        /// <param name="parser">The match predicate to be used.</param>
        /// <param name="builder">The transformation function to be applied to the result of the match.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        /// <remarks>
        /// If the match predicate fails, this match will fail as well and the transformation function will not be called.
        /// </remarks>
        public static ParseAction<TValue2> Build<TValue1, TValue2>(
            this ParseAction<TValue1> parser,
            Func<TValue1, TValue2> builder)
        {
            return input =>
            {
                var result = parser(input);
                if (result == null)
                {
                    return null;
                }

                return new ParseResult<TValue2>(result.Rest, builder(result.Value));
            };
        }

        /// <summary>
        /// Matches a predicate one or zero times.
        /// </summary>
        /// <typeparam name="TValue">The type of the match.</typeparam>
        /// <param name="parse">The match predicate to be used.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        /// <remarks>
        /// If the match cannot be performed, the match succeeds and returns the default value of the match type.
        /// The position of the parse subject is not changed when the match fails.
        /// </remarks>
        public static ParseAction<TValue> Opt<TValue>(ParseAction<TValue> parse)
        {
            return Grammar.Opt(parse);
        }

        /// <summary>
        /// Matches a chained predicate and takes the result of the left side of the match chain.
        /// </summary>
        /// <typeparam name="TLeft">The type of the left result from the match predicate.</typeparam>
        /// <typeparam name="TDown">The type of the current result from the match predicate.</typeparam>
        /// <param name="parse">The match predicate to be used.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<TLeft> Left<TLeft, TDown>(this ParseAction<Chain<TLeft, TDown>> parse)
        {
            return input =>
            {
                var result = parse(input);
                if (result == null)
                {
                    return null;
                }

                return new ParseResult<TLeft>(result.Rest, result.Value.Left);
            };
        }

        /// <summary>
        /// Matches a chained predicate and takes the current result of the match chain.
        /// </summary>
        /// <typeparam name="TLeft">The type of the left result from the match predicate.</typeparam>
        /// <typeparam name="TDown">The type of the current result from the match predicate.</typeparam>
        /// <param name="parse">The match predicate to be used.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<TDown> Down<TLeft, TDown>(this ParseAction<Chain<TLeft, TDown>> parse)
        {
            return input =>
            {
                var result = parse(input);
                if (result == null)
                {
                    return null;
                }

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
    }
}
