using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark.Compiler.ChunkVisitors
{
    public class DetectCodeExpressionVisitor : AbstractChunkVisitor
    {
        private RenderPartialChunk _currentPartial;

        public class Entry
        {
            public string Expression { get; set; }
            public bool Detected { get; set; }
        }

        readonly IList<Entry> _entries = new List<Entry>();

        public DetectCodeExpressionVisitor(RenderPartialChunk currentPartial)
        {
            _currentPartial = currentPartial;
        }

        public Entry Add(string expression)
        {
            var entry = new Entry {Expression = expression};
            _entries.Add(entry);
            return entry;
        }

        void Examine(string code)
        {
            if (string.IsNullOrEmpty(code))
                return;

            foreach(var entry in _entries)
            {
                if (entry.Detected)
                    continue;

                if (code.Contains(entry.Expression))
                    entry.Detected = true;
            }
        }

        protected override void Visit(ContentSetChunk chunk)
        {
            Accept(chunk.Body);
        }



        protected override void Visit(RenderPartialChunk chunk)
        {
            var priorPartial = _currentPartial;
            _currentPartial = chunk;
            Accept(chunk.FileContext.Contents);
            _currentPartial = priorPartial;
        }

        protected override void Visit(RenderSectionChunk chunk)
        {
            if (string.IsNullOrEmpty(chunk.Name))
            {
                Accept(_currentPartial.Body);
            }
            else if (_currentPartial.Sections.ContainsKey(chunk.Name))
            {
                Accept(_currentPartial.Sections[chunk.Name]);
            }
            else
            {
                Accept(chunk.Default);
            }
        }

        protected override void Visit(UseAssemblyChunk chunk)
        {
            //no-op
        }

        protected override void Visit(MacroChunk chunk)
        {
            //no-op
        }

        protected override void Visit(CodeStatementChunk chunk)
        {
            Examine(chunk.Code);
        }

        protected override void Visit(ExtensionChunk chunk)
        {
            // Extension content can't really be examined this way. It's too variable.
        }

        protected override void Visit(ConditionalChunk chunk)
        {
            if (!string.IsNullOrEmpty(chunk.Condition))
                Examine(chunk.Condition);
            Accept(chunk.Body);
        }

        protected override void Visit(ViewDataModelChunk chunk)
        {
            //no-op
        }

        protected override void Visit(ViewDataChunk chunk)
        {
            //no-op
        }


        protected override void Visit(AssignVariableChunk chunk)
        {
            Examine(chunk.Value);
        }

        protected override void Visit(UseContentChunk chunk)
        {
            Accept(chunk.Default);
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
            //no-op
        }

        protected override void Visit(ScopeChunk chunk)
        {
            Accept(chunk.Body);
        }

        protected override void Visit(ForEachChunk chunk)
        {
            Examine(chunk.Code);
            Accept(chunk.Body);
        }

        protected override void Visit(SendLiteralChunk chunk)
        {
            //no-op
        }

        protected override void Visit(LocalVariableChunk chunk)
        {
            Examine(chunk.Value);
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
            Examine(chunk.Code);
        }

        protected override void Visit(ContentChunk chunk)
        {
            Accept(chunk.Body);
        }

        protected override void Visit(UseNamespaceChunk chunk)
        {
            //no-op
        }
    }
}
