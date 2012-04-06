using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class IndentationVisitor : NodeVisitor<IndentationVisitor.Frame>
    {
        public class Frame
        {
            public bool BoundaryFrame { get; set; }
            public IndentationNode Indentation { get; set; }
            public IList<ElementNode> ElementsStarted { get; set; }
        }

        public IndentationVisitor(VisitorContext context)
            : base(context)
        {
        }

        protected override void Visit(IndentationNode node)
        {
            EndIndentationLength(node.Whitespace.Length);

            PushFrame(Nodes, new Frame
            {
                Indentation = node,
                ElementsStarted = new List<ElementNode>()
            });
            base.Visit(node);
        }

        private void EndIndentationLength(int length)
        {
            while (FrameData.Indentation != null && length <= FrameData.Indentation.Whitespace.Length)
            {
                foreach (var element in FrameData.ElementsStarted)
                {
                    if (Nodes.LastOrDefault() == element)
                    {
                        element.IsEmptyElement = true;
                    }
                    else
                    {
                        Nodes.Add(new EndElementNode(element.Name));
                    }
                }
                PopFrame();
            }
        }

        protected override void Visit(ElementNode node)
        {
            if (FrameData.Indentation != null)
            {
                FrameData.ElementsStarted.Add(node);
            }
            base.Visit(node);
        }

        protected override void BeforeAcceptNodes()
        {
            PushFrame(Nodes, new Frame { BoundaryFrame = true });
            base.BeforeAcceptNodes();
        }
        protected override void AfterAcceptNodes()
        {
            EndIndentationLength(0);
            if (FrameData.BoundaryFrame != true)
            {
                throw new CompilerException("Boundary frame missing");
            }
            PopFrame();
            base.AfterAcceptNodes();
        }
    }
}
