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
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class Frame<FrameData>
    {
        public Frame<FrameData> PriorFrame { get; set; }
        public IList<Node> Nodes { get; set; }
        public FrameData Data { get; set; }
    }

    public abstract class NodeVisitor<TFrameData> : AbstractNodeVisitor where TFrameData:class, new()
    {
        private Frame<TFrameData> _frame;

        protected NodeVisitor(VisitorContext context) : base(context)
        {
            PushFrame(new List<Node>(), new TFrameData());
        }

        public void PushFrame(IList<Node> nodes, TFrameData frameData)
        {
            var frame = new Frame<TFrameData> {Data = frameData, Nodes = nodes, PriorFrame = _frame};
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

        public TFrameData FrameData { get { return _frame.Data; } }

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
            PushFrame(new List<Node>(), new TFrameData());
            Accept(node.Body);
            node.Body = Nodes;
            PopFrame();
        }

        protected override void Visit(ExtensionNode node)
        {
            Nodes.Add(node);
            PushFrame(new List<Node>(), new TFrameData());
            Accept(node.Body);
            node.Body = Nodes;
            PopFrame();
        }

        protected override void Visit(ConditionNode node)
        {
            Nodes.Add(node);
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

    public class NodeVisitor : NodeVisitor<object>
    {
        public NodeVisitor(VisitorContext context) : base(context)
        {
        }
    }
}
