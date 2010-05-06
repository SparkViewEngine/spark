//-------------------------------------------------------------------------
// <copyright file="Position.cs">
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
            get
            {
                return (T)base.Value;
            }

            set
            {
                base.Value = value;
            }
        }
    }

    public class PaintLink
    {
        public PaintLink Next { get; set; }

        public Paint Paint { get; set; }
    }

    public class Position
    {
        private static readonly char[] special = { '\r', '\n', '\t' };
        private readonly int column;
        private readonly int line;
        private readonly int offset;
        private readonly SourceContext sourceContext;
        private readonly PaintLink paintLink;
        private int sourceContentLength;

        public Position(Position position)
            : this(position.SourceContext, position.sourceContentLength, position.Offset, position.Line, position.Column, position.PaintLink)
        {
        }

        public Position(SourceContext sourceContext)
            : this(sourceContext, sourceContext.Content.Length, 0, 1, 1, null)
        {
        }

        public Position(SourceContext sourceContext, int sourceContextLength, int offset, int line, int column, PaintLink paintLink)
        {
            this.sourceContext = sourceContext;
            this.sourceContentLength = sourceContextLength;
            this.offset = offset;
            this.line = line;
            this.column = column;
            this.paintLink = paintLink;
        }

        public SourceContext SourceContext
        {
            get
            {
                return this.sourceContext;
            }
        }

        public int Offset
        {
            get
            {
                return this.offset;
            }
        }

        public int Line
        {
            get
            {
                return this.line;
            }
        }

        public int Column
        {
            get
            {
                return this.column;
            }
        }

        public PaintLink PaintLink
        {
            get
            {
                return this.paintLink;
            }
        }

        public Position Advance(int count)
        {
            string content = SourceContext.Content;
            int offset = this.Offset;
            int column = this.Column;
            int line = this.Line;

            for (int remaining = count; remaining != 0;)
            {
                int specialIndex = content.IndexOfAny(special, offset, remaining) - offset;
                if (specialIndex < 0)
                {
                    // no special characters found
                    return new Position(SourceContext, this.sourceContentLength, offset + remaining, line, column + remaining, PaintLink);
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
                        throw new Exception(
                            string.Format("Unexpected character {0}", (int)content[offset + specialIndex]));
                }
            }

            return new Position(this.SourceContext, this.sourceContentLength, offset, line, column, this.PaintLink);
        }

        public Position Constrain(Position end)
        {
            return new Position(this)
            {
                sourceContentLength = end.Offset
            };
        }

        public char Peek()
        {
            if (this.Offset == this.sourceContentLength)
            {
                return default(char);
            }

            return SourceContext.Content[this.Offset];
        }
        
        public string Peek(int count)
        {
            return SourceContext.Content.Substring(this.Offset, count);
        }

        public bool PeekTest(string match)
        {
            if (this.sourceContentLength - this.Offset < match.Length)
            {
                return false;
            }

            return string.CompareOrdinal(SourceContext.Content, this.Offset, match, 0, match.Length) == 0;
        }

        public int PotentialLength()
        {
            return this.sourceContentLength - this.Offset;
        }

        public int PotentialLength(params char[] stopChars)
        {
            if (stopChars == null)
            {
                return this.PotentialLength();
            }

            int limit = this.sourceContentLength - this.Offset;
            int length = SourceContext.Content.IndexOfAny(stopChars, this.Offset, limit) - this.Offset;
            return length < 0 ? limit : length;
        }

        public Position Paint<T>(Position begin, T value)
        {
            var newPaint = new Paint<T>
            {
                Begin = begin,
                End = this,
                Value = value
            };

            var newPaintLink = new PaintLink
            {
                Next = PaintLink,
                Paint = newPaint
            };

            return new Position(
                this.sourceContext,
                this.sourceContentLength,
                this.offset,
                this.line,
                this.column,
                newPaintLink);
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
