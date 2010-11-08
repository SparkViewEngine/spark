//-------------------------------------------------------------------------
// <copyright file="CharGrammar.cs">
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
    using System.Text;

    /// <summary>
    /// Contains literal character and string matching grammar.
    /// </summary>
    public class CharGrammar : Grammar
    {
        /// <summary>
        /// Matches a character based on a predicate.  If the predicate is a match, the match succeeds.
        /// </summary>
        /// <param name="predicate">The function used to test if the character should match.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<char> Ch(Func<char, bool> predicate)
        {
            return delegate(Position input)
            {
                if (input.PotentialLength() == 0 || !predicate(input.Peek()))
                {
                   return null;
                }

                return new ParseResult<char>(input.Advance(1), input.Peek());
            };
        }

        /// <summary>
        /// Matches a character based on a predicate.  If the predicate is a match, the match fails.
        /// </summary>
        /// <param name="predicate">The function used to test if the character should match.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<char> ChNot(Func<char, bool> predicate)
        {
            return delegate(Position input)
            {
                if (input.PotentialLength() == 0 || predicate(input.Peek()))
                {
                    return null;
                }

                return new ParseResult<char>(input.Advance(1), input.Peek());
            };
        }

        /// <summary>
        /// Matches a predefined string of characters.
        /// </summary>
        /// <param name="match">The string of characters to match.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<string> Ch(string match)
        {
            return delegate(Position input)
            {
                if (!input.PeekTest(match))
                {
                    return null;
                }

                return new ParseResult<string>(input.Advance(match.Length), match);
            };
        }

        /// <summary>
        /// Matches a predefined character.
        /// </summary>
        /// <param name="allowed">The character to match.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<char> Ch(char allowed)
        {
            return delegate(Position input)
            {
                if (input.Peek() != allowed)
                {
                    return null;
                }

                return new ParseResult<char>(input.Advance(1), allowed);
            };
        }

        /// <summary>
        /// Matches a predefined set of characters.
        /// </summary>
        /// <param name="allowed">The list of characters to match.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<char> Ch(params char[] allowed)
        {
            return delegate(Position input)
            {
                var ch = input.Peek();
                if (input.PotentialLength() == 0 || !((IList<char>)allowed).Contains(ch))
                {
                    return null;
                }

                return new ParseResult<char>(input.Advance(1), ch);
            };
        }

        /// <summary>
        /// Matches a string of characters.
        /// </summary>
        /// <param name="parse">The predicate of each character to match.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<string> StringOf(ParseAction<char> parse)
        {
            return delegate(Position input)
            {
                var sb = new StringBuilder();

                var rest = input;
                var result = parse(rest);
                while (result != null)
                {
                    sb.Append(result.Value);
                    rest = result.Rest;
                    result = parse(rest);
                }

                return new ParseResult<string>(rest, sb.ToString());
            };
        }

        /// <summary>
        /// Matches any character except a predefined character.
        /// </summary>
        /// <param name="disallowed">The character disallowed for the match.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<char> ChNot(char disallowed)
        {
            return delegate(Position input)
            {
                var ch = input.Peek();
                if (ch == default(char) || ch == disallowed)
                {
                    return null;
                }

                return new ParseResult<char>(input.Advance(1), ch);
            };
        }

        /// <summary>
        /// Matches any character except a predefined set of characters.
        /// </summary>
        /// <param name="disallowed">The characters disallowed for the match.</param>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<char> ChNot(params char[] disallowed)
        {
            return delegate(Position input)
            {
                var ch = input.Peek();
                if (input.PotentialLength() == 0 || ((IList<char>)disallowed).Contains(ch))
                {
                    return null;
                }

                return new ParseResult<char>(input.Advance(1), ch);
            };
        }

        /// <summary>
        /// Matches the start or the end of the text.
        /// </summary>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<char> ChControl()
        {
            return ChSTX().Or(ChETX());
        }

        /// <summary>
        /// Matches the start of text.
        /// </summary>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<char> ChSTX()
        {
            return delegate(Position input)
            {
                if (input.Offset == 0)
                {
                    return new ParseResult<char>(input, '\u0003');
                }

                return null;
            };
        }

        /// <summary>
        /// Matches the end of the text.
        /// </summary>
        /// <returns>The corresponding ParseAction for this match.</returns>
        public static ParseAction<char> ChETX()
        {
            return delegate(Position input)
            {
                if (input.PotentialLength() == 0)
                {
                    return new ParseResult<char>(input, '\u0003');
                }

                return null;
            };
        }
    }
}