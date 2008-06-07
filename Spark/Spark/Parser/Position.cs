using System;
using System.Linq;

namespace MvcContrib.SparkViewEngine.Parser
{
	public class Position
	{
		private static readonly char[] special = "\r\n\t".ToArray();
		private readonly int _column;
		private readonly int _line;
		private readonly int _offset;
		private readonly SourceContext _sourceContext;

		public Position(Position position)
			: this(position.SourceContext, position.Offset, position.Line, position.Column)
		{
		}

		public Position(SourceContext sourceContext)
			: this(sourceContext, 0, 1, 1)
		{
		}

		public Position(SourceContext sourceContext, int offset, int line, int column)
		{
			_sourceContext = sourceContext;
			_offset = offset;
			_line = line;
			_column = column;
		}

		public SourceContext SourceContext
		{
			get { return _sourceContext; }
		}

		public int Offset
		{
			get { return _offset; }
		}

		public int Line
		{
			get { return _line; }
		}

		public int Column
		{
			get { return _column; }
		}

		public Position Advance(int count)
		{
			string content = SourceContext.Content;
			int offset = Offset;
			int column = Column;
			int line = Line;

			for (int remaining = count; remaining != 0; )
			{
				int specialIndex = content.IndexOfAny(special, offset, remaining) - offset;
				if (specialIndex < 0)
				{
					// no special characters found
					return new Position(SourceContext, offset + remaining, line, column + remaining);
				}

				switch (content[offset + specialIndex])
				{
					case '\r':
						remaining -= specialIndex + 1;
						offset += specialIndex + 1;
						column += specialIndex;
						break;
					case '\n':
						remaining -= specialIndex + 1;
						offset += specialIndex + 1;
						column = 1;
						line += 1;
						break;
					case '\t':
						remaining -= specialIndex + 1;
						offset += specialIndex + 1;

						// add any chars leading up to the tab
						column += specialIndex;

						// now add the tab effect
						column += 4 - ((column - 1) % 4);
						break;
					default:
						throw new Exception(string.Format("Unexpected character {0}",
														  (int)content[offset + specialIndex]));
				}
			}
			return new Position(SourceContext, offset, line, column);
		}

		public string Peek(int count)
		{
			return SourceContext.Content.Substring(Offset, count);
		}

		public char Peek()
		{
			if (Offset == SourceContext.Content.Length)
				return default(char);
			return SourceContext.Content[Offset];
		}

		public int PotentialLength()
		{
			return SourceContext.Content.Length - Offset;
		}

		public int PotentialLength(params char[] stopChars)
		{
			if (stopChars == null)
				return PotentialLength();

			int limit = SourceContext.Content.Length - Offset;
			int length = SourceContext.Content.IndexOfAny(stopChars, Offset, limit) - Offset;
			return length < 0 ? limit : length;
		}
	}
}