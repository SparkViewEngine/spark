/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;

namespace Spark.Parser
{
    public class CharGrammar : Grammar
    {
        public static ParseAction<char> Ch(Func<char, bool> predicate)
        {
            return delegate(Position input)
                       {
                           if (input.PotentialLength() == 0 || !predicate(input.Peek()))
                               return null;
                           return new ParseResult<char>(input.Advance(1), input.Peek());
                       };
        }

        public static ParseAction<char> ChNot(Func<char, bool> predicate)
        {
            return delegate(Position input)
                       {
                           if (input.PotentialLength() == 0 || predicate(input.Peek()))
                               return null;
                           return new ParseResult<char>(input.Advance(1), input.Peek());
                       };
        }

        public static ParseAction<string> Ch(string match)
        {
            return delegate(Position input)
                       {
                           if (input.PotentialLength() < match.Length || !string.Equals(input.Peek(match.Length), match))
                               return null;
                           return new ParseResult<string>(input.Advance(match.Length), match);
                       };
        }
        public static ParseAction<char> Ch(params char[] allowed)
        {
            return delegate(Position input)
                       {
                           if (input.PotentialLength() == 0 || !((IList<char>)allowed).Contains(input.Peek()))
                               return null;
                           return new ParseResult<char>(input.Advance(1), input.Peek());
                       };
        }
        public static ParseAction<char> ChNot(params char[] disallowed)
        {
            return delegate(Position input)
                       {
                           if (input.PotentialLength() == 0 || ((IList<char>)disallowed).Contains(input.Peek()))
                               return null;
                           return new ParseResult<char>(input.Advance(1), input.Peek());
                       };
        }
    }
}