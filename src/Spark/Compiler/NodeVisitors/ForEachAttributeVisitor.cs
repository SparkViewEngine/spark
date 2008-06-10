using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class ForEachAttributeVisitor: NodeVisitor
    {
        IList<Node> _nodes = new List<Node>();
        public IList<Node> Nodes
        {
            get { return _nodes; }
            set { _nodes = value; }
        }

        public string ClosingName { get; set; }
        public int ClosingNameOutstanding { get; set; }

        readonly Stack<Frame> _stack = new Stack<Frame>();
        class Frame
        {
            public string ClosingName { get; set; }
            public int ClosingNameOutstanding { get; set; }
            public IList<Node> Nodes { get; set; }
        }

        void PushFrame()
        {
            _stack.Push(new Frame
                            {
                                ClosingName = ClosingName,
                                ClosingNameOutstanding = ClosingNameOutstanding,
                                Nodes = Nodes
                            });
        }
        void PopFrame()
        {
            var frame = _stack.Pop();
            ClosingName = frame.ClosingName;
            ClosingNameOutstanding = frame.ClosingNameOutstanding;
            Nodes = frame.Nodes;
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
            var conditionalAttr = node.Attributes.FirstOrDefault(attr => attr.Name == "each");
            if (conditionalAttr != null)
            {
                var fakeElement = new ElementNode("for", new[] { conditionalAttr }, false);
                var specialNode = new SpecialNode(fakeElement);
                node.Attributes.Remove(conditionalAttr);
                specialNode.Body.Add(node);

                Nodes.Add(specialNode);
                if (!node.IsEmptyElement)
                {
                    PushFrame();
                    ClosingName = node.Name;
                    ClosingNameOutstanding = 1;
                    Nodes = specialNode.Body;
                }
            }
            else if (string.Equals(node.Name, ClosingName) && !node.IsEmptyElement)
            {
                ++ClosingNameOutstanding;
                Nodes.Add(node);
            }
            else
            {
                Nodes.Add(node);
            }
        }

        protected override void Visit(EndElementNode node)
        {
            Nodes.Add(node);

            if (string.Equals(node.Name, ClosingName))
            {
                --ClosingNameOutstanding;
                if (ClosingNameOutstanding == 0)
                {
                    PopFrame();
                }
            }
        }

        protected override void Visit(AttributeNode node)
        {
            Nodes.Add(node);
        }

        protected override void Visit(SpecialNode node)
        {
            var reconstructed = new SpecialNode(node.Element);

            PushFrame();

            ClosingName = null;
            Nodes = reconstructed.Body;
            Accept(node.Body);

            PopFrame();

            Nodes.Add(reconstructed);
        }

        protected override void Visit(CommentNode node)
        {
            Nodes.Add(node);
        }
    }
}
