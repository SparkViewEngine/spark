using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Parser.Code;

namespace Spark.Compiler
{
    public class SourceBuilder
    {
        public SourceBuilder(StringBuilder source)
        {
            Source = source;
            Mappings = new List<SourceMapping>();
            AdjustDebugSymbols = true;
        }

        public bool AdjustDebugSymbols { get; set; }
        public StringBuilder Source { get; set; }
        public IList<SourceMapping> Mappings { get; set; }

        public SourceBuilder AppendCode(IEnumerable<Snippet> snippets)
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
                                         OutputBegin = Source.Length,
                                         OutputEnd = Source.Length + snippet.Value.Length
                                     });
                }
                Source.Append(snippet.Value);
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

        public SourceBuilder Append(IEnumerable<Snippet> value)
        {
            return AppendCode(value);
        }

        public SourceBuilder Append(object value)
        {
            Source.Append(value);
            return this;
        }

        public SourceBuilder Append(char ch, int count)
        {
            Source.Append(ch, count);
            return this;
        }

        public SourceBuilder AppendFormat(string format, params object[] args)
        {
            Source.AppendFormat(format, args);
            return this;
        }

        public SourceBuilder AppendLine(string value)
        {
            Source.AppendLine(value);
            return this;
        }
        public SourceBuilder AppendLine()
        {
            Source.AppendLine();
            return this;
        }
    }
}
