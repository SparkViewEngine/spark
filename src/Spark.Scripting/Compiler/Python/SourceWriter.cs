using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spark.Scripting.Compiler.Python
{
    public class SourceWriter
    {
        private readonly TextWriter _writer;

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

        private void Indentation()
        {
            if (!StartOfLine) return;
            StartOfLine = false;
            _writer.Write(new string(' ', Indent));
        }
        
        public SourceWriter WriteLine()
        {
            _writer.WriteLine();
            StartOfLine = true;
            return this;
        }

        public SourceWriter Write(string value)
        {
            Indentation();
            _writer.Write(value);
            return this;
        }

        public SourceWriter WriteLine(string value)
        {
            return Write(value).WriteLine();
        }

        public SourceWriter Write(int value)
        {
            return Write(value.ToString());
        }
    }
}
