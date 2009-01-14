// Copyright 2008 Louis DeJardin - http://whereslou.com
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
using System.Linq;

namespace Spark.Parser
{
    public class Paint
    {
        public Position Begin { get; set; }
        public Position End { get; set; }
        public object Value { get; set; }
    }

    public class Paint<T> : Paint
    {
        public new T Value
        {
            get { return (T)base.Value; }
            set { base.Value = value; }
        }
    }

    public class PaintLink
    {
        public PaintLink Next { get; set; }
        public Paint Paint { get; set; }
    }

    public class Position
    {
        private static readonly char[] special = "\r\n\t".ToArray();
        private readonly int _column;
        private readonly int _line;
        private readonly int _offset;
        private readonly SourceContext _sourceContext;
        private readonly PaintLink _paintLink;
        private int _sourceContentLength;

        public Position(Position position)
            : this(position.SourceContext, position._sourceContentLength, position.Offset, position.Line, position.Column, position.PaintLink)
        {
        }

        public Position(SourceContext sourceContext)
            : this(sourceContext, sourceContext.Content.Length, 0, 1, 1, null)
        {
        }

        public Position(SourceContext sourceContext, int sourceContextLength, int offset, int line, int column, PaintLink paintLink)
        {
            _sourceContext = sourceContext;
            _sourceContentLength = sourceContextLength;
            _offset = offset;
            _line = line;
            _column = column;
            _paintLink = paintLink;
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

        public PaintLink PaintLink
        {
            get { return _paintLink; }
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
                    return new Position(SourceContext, _sourceContentLength, offset + remaining, line, column + remaining, PaintLink);
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
            return new Position(SourceContext, _sourceContentLength, offset, line, column, PaintLink);
        }

        public Position Constrain(Position end)
        {
            return new Position(this) { _sourceContentLength = end.Offset };
        }

        public string Peek(int count)
        {
            return SourceContext.Content.Substring(Offset, count);
        }

        public char Peek()
        {
            if (Offset == _sourceContentLength)
                return default(char);
            return SourceContext.Content[Offset];
        }

        public int PotentialLength()
        {
            return _sourceContentLength - Offset;
        }

        public int PotentialLength(params char[] stopChars)
        {
            if (stopChars == null)
                return PotentialLength();

            int limit = _sourceContentLength - Offset;
            int length = SourceContext.Content.IndexOfAny(stopChars, Offset, limit) - Offset;
            return length < 0 ? limit : length;
        }

        public Position Paint<T>(Position begin, T value)
        {
            return new Position(
                _sourceContext, 
                _sourceContentLength,
                _offset,
                _line,
                _column,
                new PaintLink
                    {
                        Next = PaintLink,
                        Paint = new Paint<T>
                                    {
                                        Begin = begin,
                                        End = this,
                                        Value = value
                                    }
                    });
        }

        public IEnumerable<Paint> GetPaint()
        {
            var link = PaintLink;
            while (link != null)
            {
                yield return link.Paint;
                link = link.Next;
            }
        }

    }
}
