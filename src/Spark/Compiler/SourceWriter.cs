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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark.Parser.Code;

namespace Spark.Compiler
{
    public class SourceWriter
    {
        private readonly StringWriter _writer;
        private string _escrow;

        static SourceWriter()
        {
            AdjustDebugSymbolsDefault = true;
        }

        public SourceWriter(StringWriter writer)
        {
            _writer = writer;
            Mappings = new List<SourceMapping>();
            AdjustDebugSymbols = AdjustDebugSymbolsDefault;
        }

        public int Indentation { get; set; }
        public bool StartOfLine { get; set; }
        public IList<SourceMapping> Mappings { get; set; }
        public static bool AdjustDebugSymbolsDefault { get; set; }
        public bool AdjustDebugSymbols { get; set; }

        public int Length => _writer.GetStringBuilder().Length;

        public SourceWriter AddIndent()
        {
            Indentation += 4;            
            return this;
        }

        public SourceWriter RemoveIndent()
        {
            Indentation -= 4;
            return this;
        }

        public SourceWriter Indent()
        {
            return Indent(Indentation);
        }

        public SourceWriter Indent(int size)
        {
            if (StartOfLine)
            {
                _writer.Write(new string(' ', size));
                StartOfLine = false;
            }
            return this;
        }

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
            Indent();
        }

        public SourceWriter Write(string value)
        {
            Flush();
            _writer.Write(value);
            return this;
        }

        public SourceWriter WriteFormat(string format, params object[] args)
        {
            return Write(string.Format(format, args));
        }

        public SourceWriter Write(IEnumerable<Snippet> value)
        {
            return WriteCode(value);
        }

        public SourceWriter WriteCode(IEnumerable<Snippet> snippets)
        {
            // compact snippets so vs language service doesn't have to
            var compacted = new Snippets(snippets.Count());
            Snippet prior = null;
            foreach (var snippet in snippets)
            {
                if (prior != null && SnippetAreConnected(prior, snippet))
                {
                    prior = new Snippet
                    {
                        Value = prior.Value + snippet.Value,
                        Begin = prior.Begin,
                        End = snippet.End
                    };
                    continue;
                }
                if (prior != null)
                    compacted.Add(prior);
                prior = snippet;
            }
            if (prior != null)
                compacted.Add(prior);

            // write them out and keep mapping-to-spark source information
            foreach (var snippet in compacted)
            {
                if (snippet.Begin != null)
                {
                    Mappings.Add(new SourceMapping
                    {
                        Source = snippet,
                        OutputBegin = Length,
                        OutputEnd = Length + snippet.Value.Length
                    });
                }
                Write(snippet.Value);
            }

            return this;
        }

        private static bool SnippetAreConnected(Snippet first, Snippet second)
        {
            if (first.End == null || second.Begin == null)
                return false;
            if (first.End.SourceContext != second.Begin.SourceContext)
                return false;
            if (first.End.Offset != second.Begin.Offset)
                return false;
            return true;
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

        public SourceWriter WriteLine(string format, params object[] args)
        {
            return WriteLine(string.Format(format, args));
        }

        public SourceWriter WriteDirective(string line)
        {
            if (!StartOfLine)
                WriteLine();

            StartOfLine = false;
            return WriteLine(line);
        }

        public SourceWriter WriteDirective(string format, params object[] args)
        {
            return WriteDirective(string.Format(format, args));
        }

        public SourceWriter EscrowLine(string value)
        {
            if (_escrow != null)
                _writer.Write(_escrow);

            _escrow = new string(' ', Indentation) + value + _writer.NewLine;
            return this;
        }

        public SourceWriter ClearEscrowLine()
        {
            _escrow = null;
            return this;
        }

        /// <summary>
        /// For backwards compatibility with extensions
        /// </summary>
        /// <returns>The string writer's string builder.</returns>
        public StringBuilder GetStringBuilder()
        {
            return _writer.GetStringBuilder();
        }
    }
}