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
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class SpecialNodeVisitor : AbstractNodeVisitor
    {
        private readonly IList<string> _containingNames;
        private readonly IList<string> _nonContainingNames;
        private readonly IList<string> _partialFileNames;

        private readonly ISparkExtensionFactory _extensionFactory;
        private ExtensionNode _currentExtensionNode;

        private IList<Node> _nodes = new List<Node>();
        private readonly Stack<IList<Node>> _stack = new Stack<IList<Node>>();

        public SpecialNodeVisitor(IList<string> partialFileNames, ISparkExtensionFactory extensionFactory)
        {
            _containingNames = new List<string>(new[] { "var", "def", "for", "use", "content", "test", "if", "else", "elseif", "macro", "render" });
            _nonContainingNames = new List<string>(new[] { "global", "set", "viewdata" });
            _partialFileNames = partialFileNames;
            _extensionFactory = extensionFactory;
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
            SpecialNode special = new SpecialNode(element);
            Nodes.Add(special);
            _stack.Push(Nodes);
            _nodes = special.Body;
        }

        private void PopSpecial(string name)
        {
            _nodes = _stack.Pop();
            SpecialNode special = Nodes.Last() as SpecialNode;
            if (special == null)
                throw new CompilerException(string.Format("Unexpected end element {0}", name));
            if (special.Element.Name != name)
                throw new CompilerException(string.Format("End element {0} did not match {1}", name, special.Element.Name));
        }

        protected override void Visit(ElementNode elementNode)
        {
            ISparkExtension extension;
            if (_containingNames.Contains(elementNode.Name))
            {
                PushSpecial(elementNode);
                if (elementNode.IsEmptyElement)
                    PopSpecial(elementNode.Name);
            }
            else if (_nonContainingNames.Contains(elementNode.Name))
            {
                PushSpecial(elementNode);
                PopSpecial(elementNode.Name);
            }
            else if (TryCreateExtension(elementNode, out extension))
            {
                ExtensionNode extensionNode = new ExtensionNode(elementNode, extension);
                Nodes.Add(extensionNode);

                if (!elementNode.IsEmptyElement)
                {
                    _currentExtensionNode = extensionNode;
                    _stack.Push(Nodes);
                    _nodes = extensionNode.Body;
                }
            }
            else if (_partialFileNames.Contains(elementNode.Name))
            {
                var attributes = new List<AttributeNode>(elementNode.Attributes);
                attributes.Add(new AttributeNode("file", "_" + elementNode.Name));
                var useFile = new ElementNode("use", attributes, elementNode.IsEmptyElement);
                PushSpecial(useFile);
                if (elementNode.IsEmptyElement)
                    PopSpecial("use");
            }
            else
            {
                Add(elementNode);
            }
        }

        private bool TryCreateExtension(ElementNode node, out ISparkExtension extension)
        {
            if (_extensionFactory == null)
            {
                extension = null;
                return false;
            }

            extension = _extensionFactory.CreateExtension(node);
            return extension != null;
        }


        protected override void Visit(EndElementNode endElementNode)
        {
            if (_currentExtensionNode != null &&
                string.Equals(endElementNode.Name, _currentExtensionNode.Element.Name))
            {
                _nodes = _stack.Pop();
                _currentExtensionNode = null;
            }
            else if (_containingNames.Contains(endElementNode.Name))
            {
                PopSpecial(endElementNode.Name);
            }
            else if (_nonContainingNames.Contains(endElementNode.Name))
            {
                // non-contining names are explicitly self-closing
            }
            else if (_partialFileNames.Contains(endElementNode.Name))
            {
                PopSpecial("use");
            }
            else
            {
                Add(endElementNode);
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

        protected override void Visit(ExpressionNode expressionNode)
        {
            Add(expressionNode);
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
    }
}
