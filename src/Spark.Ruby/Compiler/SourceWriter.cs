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
using System.IO;
using System.Linq;
using System.Text;
using Spark.Parser.Code;

namespace Spark.Ruby.Compiler
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