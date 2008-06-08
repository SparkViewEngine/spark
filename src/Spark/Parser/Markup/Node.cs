using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark.Parser.Markup
{
	public abstract class Node
	{
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
			Nodes = nodes;
		}
		public AttributeNode(string name, string value)
		{
			Name = name;
			Nodes = new List<Node>(new[] { new TextNode(value) });
		}

		public string Name;
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
				}
				return sb.ToString();
			}
		}
	}

	public class ExpressionNode : Node
	{
		public ExpressionNode(IList<char> code)
		{
			Code = new String(code.ToArray());
		}

		public string Code;
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

	public class ElementNode : Node
	{
		public ElementNode(string name, IList<AttributeNode> attributeNodes, bool isEmptyElement)
		{
			Name = name;
			IsEmptyElement = isEmptyElement;
			Attributes = attributeNodes;
		}

		public string Name;
		public readonly IList<AttributeNode> Attributes;
		public bool IsEmptyElement;
	}

	public class EndElementNode : Node
	{
		public EndElementNode(string name)
		{
			Name = name;
		}

		public string Name;
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
}
