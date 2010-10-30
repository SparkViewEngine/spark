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
using System.Collections.Generic;
using System.Linq;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.Parser;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class SpecialNodeVisitor : AbstractNodeVisitor
    {
        private readonly IList<string> _containingNames;
        private readonly IList<string> _nonContainingNames;

		private readonly Stack<ExtensionNode> _extensionNodes = new Stack<ExtensionNode>();

        private IList<Node> _nodes = new List<Node>();
        private readonly Stack<IList<Node>> _stack = new Stack<IList<Node>>();

        public SpecialNodeVisitor(VisitorContext context)
            : base(context)
        {
            _containingNames = new List<string>(new[] { "var", "def", "default", "for", "use", "content", "test", "if", "else", "elseif", "macro", "render", "section", "cache", "markdown" });
            _nonContainingNames = new List<string>(new[] { "global", "set", "viewdata" });
        }

        public override IList<Node> Nodes
        {
            get { return _nodes; }
        }

        private void Add(Node node)
        {
            Nodes.Add(node);
        }

        private void PushSpecial(ElementNode element)
        {
            SpecialNode special = new SpecialNode(element) { OriginalNode = element };
            Nodes.Add(special);
            _stack.Push(Nodes);
            _nodes = special.Body;
        }

        private void PopSpecial(string name)
        {
            if (_stack.Count == 0)
                throw new CompilerException(string.Format("Unexpected end element {0}", name));

            _nodes = _stack.Pop();
            SpecialNode special = Nodes.Last() as SpecialNode;
            if (special == null)
                throw new CompilerException(string.Format("Unexpected end element {0}", name));

            if (special.Element.Name != name)
                throw new CompilerException(string.Format("End element {0} did not match {1}", name, special.Element.Name));
        }

        private int _acceptNodesLevel;
        protected override void BeforeAcceptNodes()
        {
            _acceptNodesLevel++;
        }
        protected override void AfterAcceptNodes()
        {
            _acceptNodesLevel--;
            if (_acceptNodesLevel == 0)
            {
                if (_stack.Count != 0)
                {
                    var specialNode = (SpecialNode)_stack.Peek().Last();
                    var paint = Context.Paint.OfType<Paint<Node>>()
                        .FirstOrDefault(p => p.Value == specialNode.OriginalNode);
                    var position = paint == null ? null : paint.Begin;
                    throw new CompilerException(
                        string.Format("Element {0} was never closed", specialNode.Element.Name), 
                        position);
                }
            }
        }

        protected override void Visit(ElementNode node)
        {
            ISparkExtension extension;
            if (IsContainingElement(node.Name, node.Namespace))
            {
                PushSpecial(node);
                if (node.IsEmptyElement)
                    PopSpecial(node.Name);
            }
            else if (IsNonContainingElement(node.Name, node.Namespace))
            {
                PushSpecial(node);
                PopSpecial(node.Name);
            }
            else if (TryCreateExtension(node, out extension))
            {
                ExtensionNode extensionNode = new ExtensionNode(node, extension);
                Nodes.Add(extensionNode);

                if (!node.IsEmptyElement)
                {
					_extensionNodes.Push(extensionNode);
                    _stack.Push(Nodes);
                    _nodes = extensionNode.Body;
                }
            }
            else if (IsPartialFileElement(node.Name, node.Namespace))
            {
                var attributes = new List<AttributeNode>(node.Attributes);
                attributes.Add(new AttributeNode("file", "_" + NameUtility.GetName(node.Name)));
                var useFile = new ElementNode("use", attributes, node.IsEmptyElement)
                                  {
                                      OriginalNode = node
                                  };
                PushSpecial(useFile);
                if (node.IsEmptyElement)
                    PopSpecial("use");
            }
            else
            {
                Add(node);
            }
        }

        private bool IsContainingElement(string name, string ns)
        {
            if (Context.Namespaces == NamespacesType.Unqualified)
                return _containingNames.Contains(name);

            if (ns != Constants.Namespace)
                return false;

            return _containingNames.Contains(NameUtility.GetName(name));
        }

        private bool IsNonContainingElement(string name, string ns)
        {
            if (Context.Namespaces == NamespacesType.Unqualified)
                return _nonContainingNames.Contains(name);

            if (ns != Constants.Namespace)
                return false;

            return _nonContainingNames.Contains(NameUtility.GetName(name));
        }

        private bool IsPartialFileElement(string name, string ns)
        {
            if (Context.Namespaces == NamespacesType.Unqualified)
                return Context.PartialFileNames.Contains(name);

            if (ns != Constants.Namespace)
                return false;

            return Context.PartialFileNames.Contains(NameUtility.GetName(name));
        }


        private bool TryCreateExtension(ElementNode node, out ISparkExtension extension)
        {
            if (Context.ExtensionFactory == null)
            {
                extension = null;
                return false;
            }

            extension = Context.ExtensionFactory.CreateExtension(Context, node);
            return extension != null;
        }


        protected override void Visit(EndElementNode node)
        {
            if (_extensionNodes.Count > 0 &&
                string.Equals(node.Name, _extensionNodes.Peek().Element.Name))
            {
                _nodes = _stack.Pop();
				_extensionNodes.Pop();
            }
            else if (IsContainingElement(node.Name, node.Namespace))
            {
                PopSpecial(node.Name);
            }
            else if (IsNonContainingElement(node.Name, node.Namespace))
            {
                // non-contining names are explicitly self-closing
            }
            else if (IsPartialFileElement(node.Name, node.Namespace))
            {
                PopSpecial("use");
            }
            else
            {
                Add(node);
            }
        }

        protected override void Visit(AttributeNode attributeNode)
        {
            Add(attributeNode);
        }

        protected override void Visit(TextNode textNode)
        {
            Add(textNode);
        }

        protected override void Visit(ExpressionNode node)
        {
            Add(node);
        }

        protected override void Visit(EntityNode entityNode)
        {
            Add(entityNode);
        }

        protected override void Visit(DoctypeNode docTypeNode)
        {
            Add(docTypeNode);
        }

        protected override void Visit(SpecialNode specialNode)
        {
            throw new System.NotImplementedException();
        }

        protected override void Visit(ExtensionNode node)
        {
            throw new System.NotImplementedException();
        }

        protected override void Visit(CommentNode commentNode)
        {
            Add(commentNode);
        }

        protected override void Visit(StatementNode node)
        {
            Add(node);
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
