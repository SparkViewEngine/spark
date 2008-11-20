using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spark.IronRuby.Compiler.Ruby
{
    public class SourceWriter
    {
        private readonly TextWriter _writer;
        private string _escrow;

        public SourceWriter():this(new StringWriter())
        {
        }
        public SourceWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public int Indent { get; set; }
        public bool StartOfLine { get; set; }

        public override string ToString()
        {
            return _writer.ToString();
        }

        private void Flush()
        {
            if (_escrow != null)
            {
                _writer.Write(_escrow);
                _escrow = null;
            }
            if (StartOfLine)
            {
                _writer.Write(new string(' ', Indent));
                StartOfLine = false;
            }
        }
        
        public SourceWriter Write(string value)
        {
            Flush();
            _writer.Write(value);
            return this;
        }

        public SourceWriter Write(int value)
        {
            return Write(value.ToString());
        }

        public SourceWriter WriteLine()
        {
            Flush();
            _writer.WriteLine();
            StartOfLine = true;
            return this;
        }

        public SourceWriter WriteLine(string value)
        {
            return Write(value).WriteLine();
        }

        public SourceWriter WriteLine(int value)
        {
            return Write(value).WriteLine();
        }

        public void EscrowLine(string value)
        {
            if (_escrow != null)
                _writer.Write(_escrow);

            _escrow = new string(' ', Indent) + value + _writer.NewLine;
        }

        public void ClearEscrowLine()
        {
            _escrow = null;
        }
    }
}