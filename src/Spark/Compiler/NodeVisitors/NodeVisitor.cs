using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class FrameData<FrameExtra>
    {
        public FrameData<FrameExtra> PriorFrame { get; set; }
        public IList<Node> Nodes { get; set; }
        public FrameExtra Extra { get; set; }
    }

    public abstract class NodeVisitor<TExtra> : AbstractNodeVisitor where TExtra:class, new()
    {
        private FrameData<TExtra> _frame;

        protected NodeVisitor()
        {
            PushFrame(new List<Node>(), new TExtra());
        }

        public void PushFrame(IList<Node> nodes, TExtra extra)
        {
            var frame = new FrameData<TExtra> {Extra = extra, Nodes = nodes, PriorFrame = _frame};
            _frame = frame;
        }

        public void PopFrame()
        {
            _frame = _frame.PriorFrame;
        }

        public override IList<Node> Nodes
        {
            get { return _frame.Nodes; }
        }

        protected override void Visit(StatementNode node)
        {
            Nodes.Add(node);
        }

        protected override void Visit(ExpressionNode node)
        {
            Nodes.Add(node);
        }

        protected override void Visit(EntityNode node)
        {
            Nodes.Add(node);
        }

        protected override void Visit(DoctypeNode node)
        {
            Nodes.Add(node);
        }

        protected override void Visit(TextNode node)
        {
            Nodes.Add(node);
        }

        protected override void Visit(ElementNode node)
        {
            Nodes.Add(node);
        }

        protected override void Visit(EndElementNode node)
        {
            Nodes.Add(node);
        }

        protected override void Visit(AttributeNode node)
        {
            Nodes.Add(node);
        }

        protected override void Visit(CommentNode node)
        {
            Nodes.Add(node);
        }

        protected override void Visit(SpecialNode node)
        {
            Nodes.Add(node);
            PushFrame(new List<Node>(), new TExtra());
            Accept(node.Body);
            node.Body = Nodes;
            PopFrame();
        }

        protected override void Visit(ExtensionNode node)
        {
            Nodes.Add(node);
            PushFrame(new List<Node>(), new TExtra());
            Accept(node.Body);
            node.Body = Nodes;
            PopFrame();
        }
    }
}
