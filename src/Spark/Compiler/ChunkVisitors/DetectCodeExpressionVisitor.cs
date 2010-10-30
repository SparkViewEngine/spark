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
using System.Linq;
using System.Text;
using Spark.Compiler.ChunkVisitors;
using Spark.Parser.Code;

namespace Spark.Compiler.ChunkVisitors
{
    public class DetectCodeExpressionVisitor : AbstractChunkVisitor
    {
        public class Entry
        {
            public string Expression { get; set; }
            public bool Detected { get; set; }
        }

        readonly IList<Entry> _entries = new List<Entry>();

        public DetectCodeExpressionVisitor(RenderPartialChunk currentPartial)
        {
            if (currentPartial != null)
                EnterRenderPartial(currentPartial);
        }

        public Entry Add(string expression)
        {
            var entry = new Entry {Expression = expression};
            _entries.Add(entry);
            return entry;
        }

        void Examine(Snippets code)
        {
            if (Snippets.IsNullOrEmpty(code))
                return;

            var codeString = code.ToString();
            foreach(var entry in _entries)
            {
                if (entry.Detected)
                    continue;

                if (codeString.Contains(entry.Expression))
                    entry.Detected = true;
            }
        }

        protected override void Visit(UseImportChunk chunk)
        {
            
        }

        protected override void Visit(ContentSetChunk chunk)
        {
            Accept(chunk.Body);
        }


        protected override void Visit(RenderPartialChunk chunk)
        {
            EnterRenderPartial(chunk);
            Accept(chunk.FileContext.Contents);
            ExitRenderPartial(chunk);
        }

        protected override void Visit(RenderSectionChunk chunk)
        {
            var outer = ExitRenderPartial();
            if (string.IsNullOrEmpty(chunk.Name))
            {
                Accept(outer.Body);
            }
            else if (outer.Sections.ContainsKey(chunk.Name))
            {
                Accept(outer.Sections[chunk.Name]);
            }
            else
            {
                EnterRenderPartial(outer);
                Accept(chunk.Default);
                ExitRenderPartial(outer);
            }
            EnterRenderPartial(outer);
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
            if (!Snippets.IsNullOrEmpty(chunk.Condition))
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

        protected override void Visit(UseMasterChunk chunk)
        {
            //no-op
        }

        protected override void Visit(DefaultVariableChunk chunk)
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

        protected override void Visit(PageBaseTypeChunk chunk)
        {
        }

        protected override void Visit(CacheChunk chunk)
        {
            Examine(chunk.Key);
            Examine(chunk.Expires);
            Accept(chunk.Body);
        }

        protected override void Visit(MarkdownChunk chunk)
        {
            Accept(chunk.Body);
        }
    }
}