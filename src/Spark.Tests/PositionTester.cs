using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvcContrib.SparkViewEngine.Parser;
using NUnit.Framework;

namespace Spark.Tests
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

			position = position = new Position(new SourceContext("\t   \t \tx"));
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
			position = position = position.Advance(1);
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
	}
}