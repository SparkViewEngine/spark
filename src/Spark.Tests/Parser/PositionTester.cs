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
            Assert.Multiple(() =>
            {
                Assert.That(position.Line, Is.EqualTo(1));
                Assert.That(position.Column, Is.EqualTo(1));
                Assert.That(position.Offset, Is.EqualTo(0));
            });
        }

        [Test]
        public void AdvanceChangesColumnOffset()
        {
            Position position = new Position(new SourceContext("hello world"));
            Assert.Multiple(() =>
            {
                Assert.That(position.Offset, Is.EqualTo(0));
                Assert.That(position.Line, Is.EqualTo(1));
                Assert.That(position.Column, Is.EqualTo(1));
            });
            position = position.Advance(5);
            Assert.Multiple(() =>
            {
                Assert.That(position.Offset, Is.EqualTo(5));
                Assert.That(position.Line, Is.EqualTo(1));
                Assert.That(position.Column, Is.EqualTo(6));
            });
        }

        [Test]
        public void NewlineChangesLine()
        {
            Position position = new Position(new SourceContext("hello\r\nworld"));
            Assert.Multiple(() =>
            {
                Assert.That(position.Offset, Is.EqualTo(0));
                Assert.That(position.Line, Is.EqualTo(1));
                Assert.That(position.Column, Is.EqualTo(1));
            });
            position = position.Advance(5);
            Assert.Multiple(() =>
            {
                Assert.That(position.Offset, Is.EqualTo(5));
                Assert.That(position.Line, Is.EqualTo(1));
                Assert.That(position.Column, Is.EqualTo(6));
            });
            position = position.Advance(1);
            Assert.That(position.Offset, Is.EqualTo(6));
            position = position.Advance(1);
            Assert.Multiple(() =>
            {
                Assert.That(position.Offset, Is.EqualTo(7));
                Assert.That(position.Line, Is.EqualTo(2));
                Assert.That(position.Column, Is.EqualTo(1));
            });

            position = new Position(new SourceContext("hello\r\nworld"));
            position = position.Advance(9);
            Assert.Multiple(() =>
            {
                Assert.That(position.Offset, Is.EqualTo(9));
                Assert.That(position.Line, Is.EqualTo(2));
                Assert.That(position.Column, Is.EqualTo(3));
            });
        }

        [Test]
        public void UnixStyleNewlines()
        {
            Position position = new Position(new SourceContext("hello\nworld"));
            Assert.Multiple(() =>
            {
                Assert.That(position.Offset, Is.EqualTo(0));
                Assert.That(position.Line, Is.EqualTo(1));
                Assert.That(position.Column, Is.EqualTo(1));
            });
            position = position.Advance(5);
            Assert.Multiple(() =>
            {
                Assert.That(position.Offset, Is.EqualTo(5));
                Assert.That(position.Line, Is.EqualTo(1));
                Assert.That(position.Column, Is.EqualTo(6));
            });
            position = position.Advance(1);
            Assert.Multiple(() =>
            {
                Assert.That(position.Offset, Is.EqualTo(6));
                Assert.That(position.Line, Is.EqualTo(2));
                Assert.That(position.Column, Is.EqualTo(1));
            });
            position = position.Advance(1);
            Assert.Multiple(() =>
            {
                Assert.That(position.Offset, Is.EqualTo(7));
                Assert.That(position.Line, Is.EqualTo(2));
                Assert.That(position.Column, Is.EqualTo(2));
            });

            position = new Position(new SourceContext("hello\nworld"));
            position = position.Advance(9);
            Assert.Multiple(() =>
            {
                Assert.That(position.Offset, Is.EqualTo(9));
                Assert.That(position.Line, Is.EqualTo(2));
                Assert.That(position.Column, Is.EqualTo(4));
            });
        }

        [Test]
        public void PeekReturnsText()
        {
            Position position = new Position(new SourceContext("hello\r\nworld"));
            Assert.That(position.Peek(5), Is.EqualTo("hello"));
            position = position.Advance(7);
            Assert.Multiple(() =>
            {
                Assert.That(position.Peek(1), Is.EqualTo("w"));
                Assert.That(position.Peek(5), Is.EqualTo("world"));
                Assert.That(position.Peek(), Is.EqualTo('w'));
            });
        }

        [Test]
        public void FourSpaceTabs()
        {
            Position position = new Position(new SourceContext("\t\tx"));
            Assert.That(position.Column, Is.EqualTo(1));
            position = position.Advance(1);
            Assert.That(position.Column, Is.EqualTo(5));
            position = position.Advance(1);
            Assert.That(position.Column, Is.EqualTo(9));

            position = new Position(new SourceContext("\t   \t \tx"));
            Assert.That(position.Column, Is.EqualTo(1));
            position = position.Advance(1);
            Assert.That(position.Column, Is.EqualTo(5));
            position = position.Advance(1);
            Assert.That(position.Column, Is.EqualTo(6));
            position = position.Advance(1);
            Assert.That(position.Column, Is.EqualTo(7));
            position = position.Advance(1);
            Assert.That(position.Column, Is.EqualTo(8));
            position = position.Advance(1);
            Assert.That(position.Column, Is.EqualTo(9));
            position = position.Advance(1);
            Assert.That(position.Column, Is.EqualTo(10));
            position = position.Advance(1);
            Assert.That(position.Column, Is.EqualTo(13));
        }

        [Test]
        public void PotentialLengths()
        {
            Position position = new Position(new SourceContext("hello world"));
            Assert.Multiple(() =>
            {
                Assert.That(position.PotentialLength(), Is.EqualTo(11));
                Assert.That(position.PotentialLength('w'), Is.EqualTo(6));
                Assert.That(position.PotentialLength('l'), Is.EqualTo(2));
                Assert.That(position.PotentialLength('h'), Is.EqualTo(0));
            });
            position = position.Advance(4);
            Assert.Multiple(() =>
            {
                Assert.That(position.PotentialLength(), Is.EqualTo(7));
                Assert.That(position.PotentialLength('w'), Is.EqualTo(2));
                Assert.That(position.PotentialLength('l'), Is.EqualTo(5));
                Assert.That(position.PotentialLength('o'), Is.EqualTo(0));
            });
            position = position.Advance(7);
            Assert.Multiple(() =>
            {
                Assert.That(position.PotentialLength(), Is.EqualTo(0));
                Assert.That(position.PotentialLength('w'), Is.EqualTo(0));
                Assert.That(position.PotentialLength('l'), Is.EqualTo(0));
            });
        }

        [Test]
        public void PotentialLengthMultiChar()
        {
            Position position = new Position(new SourceContext("hello world"));
            Assert.Multiple(() =>
            {
                Assert.That(position.PotentialLength(), Is.EqualTo(11));
                Assert.That(position.PotentialLength('h', 'w', 'l'), Is.EqualTo(0));
                Assert.That(position.PotentialLength('l', 'w', 'h'), Is.EqualTo(0));
                Assert.That(position.PotentialLength('i', 'f'), Is.EqualTo(11));
                Assert.That(position.PotentialLength('b', 'o', 'o'), Is.EqualTo(4));
            });
            position = position.Advance(2);
            Assert.Multiple(() =>
            {
                Assert.That(position.PotentialLength('h', 'w', 'l'), Is.EqualTo(0));
                Assert.That(position.PotentialLength('l', 'w', 'h'), Is.EqualTo(0));
                Assert.That(position.PotentialLength('i', 'f'), Is.EqualTo(9));
                Assert.That(position.PotentialLength('b', 'o', 'o'), Is.EqualTo(2));
            });
        }

        [Test]
        public void DefaultEndOfSourceCharacter()
        {
            var position = new Position(new SourceContext("hello world"));
            var pos2 = position.Advance(11);
            Assert.That(pos2.Peek(), Is.EqualTo(default(char)));
        }

        [Test]
        public void PotentialLengthConstrained()
        {
            Position position = new Position(new SourceContext("hello world"));
            var begin = position.Advance(4);
            var end = begin.Advance(3);
            var range = begin.Constrain(end);
            Assert.Multiple(() =>
            {
                Assert.That(range.PotentialLength(), Is.EqualTo(3));
                Assert.That(range.Peek(3), Is.EqualTo("o w"));
            });
            var done = range.Advance(3);
            Assert.That(done.Peek(), Is.EqualTo(default(char)));
        }

        [Test]
        public void PeekTestChecksNextCharacters()
        {
            var position = new Position(new SourceContext("hello world"));
            Assert.Multiple(() =>
            {
                Assert.That(position.PeekTest("hello"), Is.True);
                Assert.That(position.PeekTest("hello world"), Is.True);
                Assert.That(position.PeekTest("hello world!"), Is.False);
                Assert.That(position.PeekTest("Hello"), Is.False);
                Assert.That(position.PeekTest(""), Is.True);
            });
            var more = position.Advance(4);
            Assert.Multiple(() =>
            {
                Assert.That(more.PeekTest("hello"), Is.False);
                Assert.That(more.PeekTest("o"), Is.True);
                Assert.That(more.PeekTest("o world"), Is.True);
                Assert.That(more.PeekTest("o world!"), Is.False);
            });
            var tail = more.Advance(7);
            Assert.Multiple(() =>
            {
                Assert.That(tail.PeekTest(""), Is.True);
                Assert.That(tail.PeekTest("d"), Is.False);
            });

        }
    }
}