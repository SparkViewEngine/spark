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
using System.IO;
using Spark.Parser.Code;

namespace Spark.Python.Compiler
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