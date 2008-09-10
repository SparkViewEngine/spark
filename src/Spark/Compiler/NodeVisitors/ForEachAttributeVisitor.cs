/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Collections.Generic;
using System.Linq;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class ForEachAttributeVisitor : AbstractNodeVisitor
    {
        IList<Node> _nodes = new List<Node>();

        public ForEachAttributeVisitor(VisitorContext context)
            : base(context)
        {
        }

        public override IList<Node> Nodes
        {
            get { return _nodes; }
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
            _nodes = frame.Nodes;
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

        bool IsEachAttribute(AttributeNode attr)
        {
            if (Context.Namespaces == NamespacesType.Unqualified)
                return attr.Name == "each";

            if (attr.Namespace != Constants.Namespace)
                return false;

            return NameUtility.GetName(attr.Name) == "each";
        }

        protected override void Visit(ElementNode node)
        {
            var eachAttr = node.Attributes.FirstOrDefault(IsEachAttribute);
            if (eachAttr != null)
            {
                var fakeAttribute = new AttributeNode("each", eachAttr.Nodes) { OriginalNode = eachAttr };
                var fakeElement = new ElementNode("for", new[] { fakeAttribute }, false) { OriginalNode = eachAttr };
                var specialNode = new SpecialNode(fakeElement);
                node.Attributes.Remove(eachAttr);
                specialNode.Body.Add(node);

                Nodes.Add(specialNode);
                if (!node.IsEmptyElement)
                {
                    PushFrame();
                    ClosingName = node.Name;
                    ClosingNameOutstanding = 1;
                    _nodes = specialNode.Body;
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
            _nodes = reconstructed.Body;
            Accept(node.Body);

            PopFrame();

            Nodes.Add(reconstructed);
        }

        protected override void Visit(ExtensionNode node)
        {
            var reconstructed = new ExtensionNode(node.Element, node.Extension);

            PushFrame();

            ClosingName = null;
            _nodes = reconstructed.Body;
            Accept(node.Body);

            PopFrame();

            Nodes.Add(reconstructed);
        }

        protected override void Visit(CommentNode node)
        {
            Nodes.Add(node);
        }

        protected override void Visit(StatementNode node)
        {
            Nodes.Add(node);
        }

        protected override void Visit(ConditionNode node)
        {
            throw new System.NotImplementedException();
        }

        protected override void Visit(XMLDeclNode node)
        {
            Nodes.Add(node);
        }

        protected override void Visit(ProcessingInstructionNode node)
        {
            Nodes.Add(node);
        }
    }
}
