//-------------------------------------------------------------------------
// <copyright file="Grammar.cs">
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
    using System.Collections.Generic;

    /// <summary>
    /// Contains parse actions shared accross all grammars.
    /// </summary>
    public abstract class Grammar
    {
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
            ParseAction<TValue1> p1,
            ParseAction<TValue2> p2)
        {
            return delegate(Position input)
            {
                var r1 = p1(input);
                if (r1 == null)
                {
                    return null;
                }

                var r2 = p2(r1.Rest);
                if (r2 == null)
                {
                    return null;
                }

                var chain = new Chain<TValue1, TValue2>(
                    r1.Value,
                    r2.Value);

                return new ParseResult<Chain<TValue1, TValue2>>(
                    r2.Rest,
                    chain);
            };
        }

        /// <summary>
        /// Indicates a disjunction (logical or) between two matches.
        /// </summary>
        /// <typeparam name="TValue">The type of the matches.</typeparam>
        /// <param name="p1">The first option in the disjunction.</param>
        /// <param name="p2">The second option in the disjunction.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<TValue> Or<TValue>(
            ParseAction<TValue> p1,
            ParseAction<TValue> p2)
        {
            return input => p1(input) ?? p2(input);
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
            ParseAction<TValue1> p1,
            ParseAction<TValue2> p2)
        {
            return delegate(Position input)
            {
                var r1 = p1(input);
                if (r1 == null)
                {
                    return null;
                }

                var r2 = p2(input);
                if (r2 != null)
                {
                    return null;
                }

                return r1;
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
            return input => parse(input) ?? new ParseResult<TValue>(input, default(TValue));
        }

        /// <summary>
        /// Uses the first match, as long as the second match succeeds immediately after the first.
        /// </summary>
        /// <typeparam name="TValue">The type of the first match.</typeparam>
        /// <typeparam name="TValue2">The type of the second match.</typeparam>
        /// <param name="parse">The match predicate to be used.</param>
        /// <param name="cond">The match predicate that must match immediately after <paramref name="parse"/>.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<TValue> IfNext<TValue, TValue2>(ParseAction<TValue> parse, ParseAction<TValue2> cond)
        {
            return delegate(Position input)
            {
                var result = parse(input);
                if (result == null || cond(result.Rest) == null)
                {
                    return null;
                }

                return result;
            };
        }

        /// <summary>
        /// Uses the first match, as long as the second match does not succeed immediately after the first.
        /// </summary>
        /// <typeparam name="TValue">The type of the first match.</typeparam>
        /// <typeparam name="TValue2">The type of the second match.</typeparam>
        /// <param name="parse">The match predicate to be used.</param>
        /// <param name="cond">The match predicate that must not match immediately after <paramref name="parse"/>.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<TValue> NotNext<TValue, TValue2>(ParseAction<TValue> parse, ParseAction<TValue2> cond)
        {
            return delegate(Position input)
            {
                var result = parse(input);
                if (result == null || cond(result.Rest) != null)
                {
                    return null;
                }

                return result;
            };
        }

        /// <summary>
        /// Repeats a match zero or more times.
        /// </summary>
        /// <typeparam name="TValue">The type of the match.</typeparam>
        /// <param name="parse">The match predicate to be repeated.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<IList<TValue>> Rep<TValue>(ParseAction<TValue> parse)
        {
            return delegate(Position input)
            {
                var list = new List<TValue>();

                var rest = input;
                var result = parse(rest);
                while (result != null)
                {
                    list.Add(result.Value);
                    rest = result.Rest;
                    result = parse(rest);
                }

                return new ParseResult<IList<TValue>>(rest, list);
            };
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
        public static ParseAction<IList<TValue>> Rep1<TValue>(ParseAction<TValue> parse)
        {
            return delegate(Position input)
            {
                var rest = input;
                var result = parse(rest);
                if (result == null)
                {
                    return null;
                }

                var list = new List<TValue>();
                while (result != null)
                {
                    rest = result.Rest;
                    list.Add(result.Value);
                    result = parse(rest);
                }

                return new ParseResult<IList<TValue>>(rest, list);
            };
        }

        public static ParseAction<TValue> Paint<TValue>(ParseAction<TValue> parser)
        {
            return position =>
            {
                var result = parser(position);
                if (result == null)
                {
                    return null;
                }

                return new ParseResult<TValue>(result.Rest.Paint(position, result.Value), result.Value);
            };

            // ${hello[[world]]}
            //        <
            //               >
            //  "hello<world>"
        }

        public static ParseAction<TValue> Paint<TValue, TPaintValue>(ParseAction<TValue> parser) where TValue : TPaintValue
        {
            return position =>
            {
                var result = parser(position);
                if (result == null)
                {
                    return null;
                }

                return new ParseResult<TValue>(result.Rest.Paint<TPaintValue>(position, result.Value), result.Value);
            };
        }

        public static ParseAction<TValue> Paint<TValue, TPaintValue>(TPaintValue value, ParseAction<TValue> parser)
        {
            return position =>
            {
                var result = parser(position);
                if (result == null)
                {
                    return null;
                }

                return new ParseResult<TValue>(result.Rest.Paint(position, value), result.Value);
            };
        }
    }
}
