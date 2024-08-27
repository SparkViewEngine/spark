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

using Spark.Parser;
using NUnit.Framework;

namespace Spark.Tests.Parser
{
    [TestFixture, Category("SparkViewEngine")]
    public class PositionTester
    {
        [Test]
        public void PositionStartsOnOneOne()
        {
            Position position = new Position(new SourceContext("hello world"));
            Assert.AreEqual(1, position.Line);
            Assert.AreEqual(1, position.Column);
            Assert.AreEqual(0, position.Offset);
        }

        [Test]
        public void AdvanceChangesColumnOffset()
        {
            Position position = new Position(new SourceContext("hello world"));
            Assert.AreEqual(0, position.Offset);
            Assert.AreEqual(1, position.Line);
            Assert.AreEqual(1, position.Column);
            position = position.Advance(5);
            Assert.AreEqual(5, position.Offset);
            Assert.AreEqual(1, position.Line);
            Assert.AreEqual(6, position.Column);
        }

        [Test]
        public void NewlineChangesLine()
        {
            Position position = new Position(new SourceContext("hello\r\nworld"));
            Assert.AreEqual(0, position.Offset);
            Assert.AreEqual(1, position.Line);
            Assert.AreEqual(1, position.Column);
            position = position.Advance(5);
            Assert.AreEqual(5, position.Offset);
            Assert.AreEqual(1, position.Line);
            Assert.AreEqual(6, position.Column);
            position = position.Advance(1);
            Assert.AreEqual(6, position.Offset);
            position = position.Advance(1);
            Assert.AreEqual(7, position.Offset);
            Assert.AreEqual(2, position.Line);
            Assert.AreEqual(1, position.Column);

            position = new Position(new SourceContext("hello\r\nworld"));
            position = position.Advance(9);
            Assert.AreEqual(9, position.Offset);
            Assert.AreEqual(2, position.Line);
            Assert.AreEqual(3, position.Column);
        }

        [Test]
        public void UnixStyleNewlines()
        {
            Position position = new Position(new SourceContext("hello\nworld"));
            Assert.AreEqual(0, position.Offset);
            Assert.AreEqual(1, position.Line);
            Assert.AreEqual(1, position.Column);
            position = position.Advance(5);
            Assert.AreEqual(5, position.Offset);
            Assert.AreEqual(1, position.Line);
            Assert.AreEqual(6, position.Column);
            position = position.Advance(1);
            Assert.AreEqual(6, position.Offset);
            Assert.AreEqual(2, position.Line);
            Assert.AreEqual(1, position.Column);
            position = position.Advance(1);
            Assert.AreEqual(7, position.Offset);
            Assert.AreEqual(2, position.Line);
            Assert.AreEqual(2, position.Column);

            position = new Position(new SourceContext("hello\nworld"));
            position = position.Advance(9);
            Assert.AreEqual(9, position.Offset);
            Assert.AreEqual(2, position.Line);
            Assert.AreEqual(4, position.Column);
        }

        [Test]
        public void PeekReturnsText()
        {
            Position position = new Position(new SourceContext("hello\r\nworld"));
            Assert.AreEqual("hello", position.Peek(5));
            position = position.Advance(7);
            Assert.AreEqual("w", position.Peek(1));
            Assert.AreEqual("world", position.Peek(5));
            Assert.AreEqual('w', position.Peek());
        }

        [Test]
        public void FourSpaceTabs()
        {
            Position position = new Position(new SourceContext("\t\tx"));
            Assert.AreEqual(1, position.Column);
            position = position.Advance(1);
            Assert.AreEqual(5, position.Column);
            position = position.Advance(1);
            Assert.AreEqual(9, position.Column);

            position = new Position(new SourceContext("\t   \t \tx"));
            Assert.AreEqual(1, position.Column);
            position = position.Advance(1);
            Assert.AreEqual(5, position.Column);
            position = position.Advance(1);
            Assert.AreEqual(6, position.Column);
            position = position.Advance(1);
            Assert.AreEqual(7, position.Column);
            position = position.Advance(1);
            Assert.AreEqual(8, position.Column);
            position = position.Advance(1);
            Assert.AreEqual(9, position.Column);
            position = position.Advance(1);
            Assert.AreEqual(10, position.Column);
            position = position.Advance(1);
            Assert.AreEqual(13, position.Column);
        }

        [Test]
        public void PotentialLengths()
        {
            Position position = new Position(new SourceContext("hello world"));
            Assert.AreEqual(11, position.PotentialLength());
            Assert.AreEqual(6, position.PotentialLength('w'));
            Assert.AreEqual(2, position.PotentialLength('l'));
            Assert.AreEqual(0, position.PotentialLength('h'));
            position = position.Advance(4);
            Assert.AreEqual(7, position.PotentialLength());
            Assert.AreEqual(2, position.PotentialLength('w'));
            Assert.AreEqual(5, position.PotentialLength('l'));
            Assert.AreEqual(0, position.PotentialLength('o'));
            position = position.Advance(7);
            Assert.AreEqual(0, position.PotentialLength());
            Assert.AreEqual(0, position.PotentialLength('w'));
            Assert.AreEqual(0, position.PotentialLength('l'));
        }

        [Test]
        public void PotentialLengthMultiChar()
        {
            Position position = new Position(new SourceContext("hello world"));
            Assert.AreEqual(11, position.PotentialLength());
            Assert.AreEqual(0, position.PotentialLength('h', 'w', 'l'));
            Assert.AreEqual(0, position.PotentialLength('l', 'w', 'h'));
            Assert.AreEqual(11, position.PotentialLength('i', 'f'));
            Assert.AreEqual(4, position.PotentialLength('b', 'o', 'o'));
            position = position.Advance(2);
            Assert.AreEqual(0, position.PotentialLength('h', 'w', 'l'));
            Assert.AreEqual(0, position.PotentialLength('l', 'w', 'h'));
            Assert.AreEqual(9, position.PotentialLength('i', 'f'));
            Assert.AreEqual(2, position.PotentialLength('b', 'o', 'o'));
        }

        [Test]
        public void DefaultEndOfSourceCharacter()
        {
            var position = new Position(new SourceContext("hello world"));
            var pos2 = position.Advance(11);
            Assert.AreEqual(default(char), pos2.Peek());
        }

        [Test]
        public void PotentialLengthConstrained()
        {
            Position position = new Position(new SourceContext("hello world"));
            var begin = position.Advance(4);
            var end = begin.Advance(3);
            var range = begin.Constrain(end);
            Assert.AreEqual(3, range.PotentialLength());
            Assert.AreEqual("o w", range.Peek(3));
            var done = range.Advance(3);
            Assert.AreEqual(default(char), done.Peek());
        }

        [Test]
        public void PeekTestChecksNextCharacters()
        {
            var position = new Position(new SourceContext("hello world"));
            Assert.That(position.PeekTest("hello"), Is.True);
            Assert.That(position.PeekTest("hello world"), Is.True);
            Assert.That(position.PeekTest("hello world!"), Is.False);
            Assert.That(position.PeekTest("Hello"), Is.False);
            Assert.That(position.PeekTest(""), Is.True);
            var more = position.Advance(4);
            Assert.That(more.PeekTest("hello"), Is.False);
            Assert.That(more.PeekTest("o"), Is.True);
            Assert.That(more.PeekTest("o world"), Is.True);
            Assert.That(more.PeekTest("o world!"), Is.False);
            var tail = more.Advance(7);
            Assert.That(tail.PeekTest(""), Is.True);
            Assert.That(tail.PeekTest("d"), Is.False);

        }
    }
}