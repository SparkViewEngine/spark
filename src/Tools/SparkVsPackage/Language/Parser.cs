using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Spark.Parser;
using Spark.Parser.Markup;

namespace SparkVsPackage.Language
{
    public class Parser
    {
        MarkupGrammar _grammar = new MarkupGrammar();
        private string _content;
        private ParseResult<IList<Node>> _results;

        public Parser(IVsTextLines buffer)
        {
            Buffer = buffer;
        }
        public Parser(string text)
        {
            Text = text;
        }

        public IVsTextLines Buffer { get; set; }
        public string Text { get; set; }

        public void Refresh()
        {
            string content;

            if (Buffer == null)
            {
                content = Text;
            }
            else
            {
                int iLastLine;
                int iLastLength;
                Buffer.GetLineCount(out iLastLine);
                Buffer.GetLengthOfLine(iLastLine - 1, out iLastLength);
                Buffer.GetLineText(0, 0, iLastLine - 1, iLastLength, out content);
            }

            if (_content == content) 
                return;

            _results = _grammar.Nodes(new Position(new SourceContext(content)));
            _content = content;
        }

        public IEnumerable<Paint> GetPaint()
        {
            return _results.Rest.GetPaint();
        }
    }
}
