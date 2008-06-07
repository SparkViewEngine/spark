using System;
using System.Collections.Generic;
using System.Text;
using MvcContrib.SparkViewEngine.Parser.Markup;

namespace MvcContrib.SparkViewEngine.Compiler.NodeVisitors
{
	public abstract class NodeVisitor
	{
		public void Accept(IList<Node> nodes)
		{
			foreach (var node in nodes)
				Accept(node);
		}

		public void Accept(Node node)
		{
			if (node is TextNode)
				Visit((TextNode)node);
			else if (node is EntityNode)
				Visit((EntityNode)node);
			else if (node is ExpressionNode)
				Visit((ExpressionNode)node);
			else if (node is ElementNode)
				Visit((ElementNode)node);
			else if (node is AttributeNode)
				Visit((AttributeNode)node);
			else if (node is EndElementNode)
				Visit((EndElementNode)node);
			else if (node is DoctypeNode)
				Visit((DoctypeNode)node);
			else if (node is CommentNode)
				Visit((CommentNode)node);
			else if (node is SpecialNode)
				Visit((SpecialNode)node);
			else
				throw new ArgumentException(string.Format("Unknown node type {0}", node.GetType()), "node");
		}


		protected abstract void Visit(ExpressionNode expressionNode);
		protected abstract void Visit(EntityNode entityNode);
		protected abstract void Visit(DoctypeNode docTypeNode);
		protected abstract void Visit(TextNode textNode);
		protected abstract void Visit(ElementNode elementNode);
		protected abstract void Visit(EndElementNode endElementNode);
		protected abstract void Visit(AttributeNode attributeNode);
		protected abstract void Visit(SpecialNode specialNode);
		protected abstract void Visit(CommentNode commentNode);
	}
}