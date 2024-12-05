// Copyright 2008-2024 Louis DeJardin
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
using System.Collections.Generic;
using NUnit.Framework;

using Spark.Parser;
using Spark.Parser.Code;
using System.Linq;

namespace Spark.Tests.Parser
{
    [TestFixture]
    public class CharGrammarTester
    {
        private CharGrammarAdapter _grammar;

        class CharGrammarAdapter : CharGrammar
        {
            public CharGrammarAdapter()
            {
                AllText = Rep(Ch(ch => true));
                ChNotToEnd = Rep(ChNot());
                ChNotDashToEnd = Rep(ChNot('-'));
            }

            public ParseAction<IList<char>> AllText { get; set; }
            public ParseAction<IList<char>> ChNotToEnd { get; set; }
            public ParseAction<IList<char>> ChNotDashToEnd { get; set; }
        }

        [SetUp]
        public void Init()
        {
            _grammar = new CharGrammarAdapter();
        }

        static Position Source(string text)
        {
            return new Position(new SourceContext(text));
        }

        static string Combine(IEnumerable<char> value)
        {
            return new string(value.ToArray());
        }

        [Test]
        public void AllText()
        {
            var result = _grammar.AllText(Source("hello world"));
            Assert.That(Combine(result.Value), Is.EqualTo("hello world"));
        }

        [Test]
        public void ChNotToEnd()
        {
            var result = _grammar.ChNotToEnd(Source("hello world"));
            Assert.That(Combine(result.Value), Is.EqualTo("hello world"));
        }

        [Test]
        public void ChNotDashToEnd()
        {
            var result = _grammar.ChNotDashToEnd(Source("hello world"));
            Assert.That(Combine(result.Value), Is.EqualTo("hello world"));
        }
    }
}