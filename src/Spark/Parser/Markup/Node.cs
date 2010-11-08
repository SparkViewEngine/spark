//-------------------------------------------------------------------------
// <copyright file="Node.cs">
// Copyright 2008-2010 Louis DeJardin - http://whereslou.com
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
// </copyright>
// <author>Louis DeJardin</author>
// <author>John Gietzen</author>
//-------------------------------------------------------------------------

namespace Spark.Parser.Markup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Spark.Parser.Code;

    public abstract class Node
    {
        public Node OriginalNode { get; set; }
    }

    public class TextNode : Node
    {
        public TextNode(ICollection<char> text)
        {
            Text = new string(text.ToArray());
        }
        public TextNode(string text)
        {
            Text = text;
        }

        public string Text;
    }

    public class EntityNode : Node
    {
        public EntityNode(string name)
        {
            Name = name;
        }

        public string Name;
    }

    public class CommentNode : Node
    {
        public CommentNode(IList<char> text)
        {
            Text = new string(text.ToArray());
        }
        public CommentNode(string text)
        {
            Text = text;
        }
        public string Text { get; set; }
    }

    public class AttributeNode : Node
    {
        public AttributeNode(string name, IList<Node> nodes)
        {
            Name = name;
            Namespace = "";
            Nodes = nodes;
        }
        public AttributeNode(string name, string value)
        {
            Name = name;
            Namespace = "";
            Nodes = new List<Node>(new[] { new TextNode(value) });
        }

        public string Name;
        public string Namespace { get; set; }
        public IList<Node> Nodes;

        public string Value
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var node in Nodes)
                {
                    if (node is TextNode)
                        sb.Append(((TextNode)node).Text);
                    else if (node is EntityNode)
                        sb.Append('&').Append(((EntityNode)node).Name).Append(';');
                    else if (node is ExpressionNode)
                        sb.Append("${").Append(((ExpressionNode)node).Code).Append('}');
                    else if (node is ConditionNode)
                        sb.Append("?{").Append(((ConditionNode)node).Code).Append('}');
                }
                return sb.ToString();
            }
        }

    }

    public class ConditionNode : Node
    {
        public ConditionNode()
        {
            Nodes = new List<Node>();
        }

        public ConditionNode(string code)
            : this()
        {
            Code = new Snippets(new[] { new Snippet { Value = code } });
        }

        public ConditionNode(IEnumerable<Snippet> snippets)
            : this()
        {
            Code = new Snippets(snippets);
        }
        public ConditionNode(Snippets snippets)
            : this()
        {
            Code = snippets;
        }

        public Snippets Code { get; set; }
        public IList<Node> Nodes { get; set; }
    }

    public class ExpressionNode : Node
    {
        public ExpressionNode(string code)
            : this(new Snippets(code))
        {
        }
        public ExpressionNode(IEnumerable<Snippet> code) :
            this(new Snippets(code))
        {
        }
        public ExpressionNode(Snippets code)
        {
            Code = code;
        }

        public Snippets Code { get; set; }
        public bool SilentNulls { get; set; }
        public bool AutomaticEncoding { get; set; }
    }

    public class StatementNode : Node
    {
        public StatementNode(string code) :
            this(new Snippets(code))
        {
        }
        public StatementNode(IEnumerable<Snippet> snippets) :
            this(new Snippets(snippets))
        {
        }
        public StatementNode(Snippets code)
        {
            Code = code;
        }


        public Snippets Code { get; set; }
    }

    public class ExternalIdInfo
    {
        public string ExternalIdType;
        public string PublicId;
        public string SystemId;
    }

    public class DoctypeNode : Node
    {
        public string Name;

        public ExternalIdInfo ExternalId;
    }

    public class XMLDeclNode : Node
    {
        public string Encoding { get; set; }

        public string Standalone { get; set; }
    }

    public class ProcessingInstructionNode : Node
    {
        public string Name { get; set; }
        public string Body { get; set; }
    }

    public class ElementNode : Node
    {
        public ElementNode(string name, IList<AttributeNode> attributeNodes, bool isEmptyElement)
            : this(name, attributeNodes, isEmptyElement, string.Empty)
        {
        }

        public ElementNode(string name, IList<AttributeNode> attributeNodes, bool isEmptyElement, string preceedingWhitespace)
        {
            Name = name;
            Namespace = "";
            IsEmptyElement = isEmptyElement;
            Attributes = attributeNodes;
            PreceedingWhitespace = preceedingWhitespace;
        }

        public string Name { get; set; }
        public string Namespace { get; set; }
        public readonly IList<AttributeNode> Attributes;
        public bool IsEmptyElement { get; set; }
        public string PreceedingWhitespace { get; set; }
    }

    public class EndElementNode : Node
    {
        public EndElementNode(string name)
            : this(name, string.Empty)
        {
        }

        public EndElementNode(string name, string preceedingWhitespace)
        {
            Name = name;
            PreceedingWhitespace = preceedingWhitespace;
        }

        public string Name { get; set; }
        public string Namespace { get; set; }
        public string PreceedingWhitespace { get; set; }
    }

    public class SpecialNode : Node
    {
        public SpecialNode(ElementNode element)
        {
            Element = element;
        }

        public ElementNode Element;
        public IList<Node> Body = new List<Node>();
    }

    public class ExtensionNode : Node
    {
        public ExtensionNode(ElementNode element, ISparkExtension extension)
        {
            Element = element;
            Extension = extension;
            OriginalNode = element;
        }

        public ElementNode Element;
        public ISparkExtension Extension { get; set; }
        public IList<Node> Body = new List<Node>();
    }
}
