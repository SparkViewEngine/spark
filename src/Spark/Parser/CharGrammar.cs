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
            if (match.Length < 2)
            {
                int x = 5;
            }
            return delegate(Position input)
                       {
                           if (!input.PeekTest(match))
                               return null;
                           return new ParseResult<string>(input.Advance(match.Length), match);
                       };
        }

        public static ParseAction<char> Ch(char allowed)
        {
            return delegate(Position input)
            {
                if (input.Peek() != allowed)
                    return null;
                return new ParseResult<char>(input.Advance(1), allowed);
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
        public static ParseAction<char> ChNot(char disallowed)
        {
            return delegate(Position input)
            {
                var ch = input.Peek();
                if (ch == disallowed)
                    return null;
                return new ParseResult<char>(input.Advance(1), ch);
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

        public static ParseAction<char> ChControl()
        {
            return delegate(Position input)
                       {
                           // STX at start of file
                           if (input.Offset == 0) 
                               return new ParseResult<char>(input, '\u0002');

                           // ETX at end of file
                           if (input.PotentialLength() == 0)
                               return new ParseResult<char>(input, '\u0003');

                           return null;
                       };
        }

        public static ParseAction<char> ChSTX()
        {
            var chControl = ChControl();
            return delegate(Position input)
                       {
                           var result = chControl(input);
                           if (result == null || result.Value != '\u0002')
                               return null;
                           return result;
                       };
        }

        public static ParseAction<char> ChETX()
        {
            var chControl = ChControl();
            return delegate(Position input)
            {
                var result = chControl(input);
                if (result == null || result.Value != '\u0003')
                    return null;
                return result;
            };
        }
    }
}