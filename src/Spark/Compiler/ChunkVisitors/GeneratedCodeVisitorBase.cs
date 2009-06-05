using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark.Compiler.ChunkVisitors
{
    public class GeneratedCodeVisitorBase : ChunkVisitor
    {
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
    }
}
