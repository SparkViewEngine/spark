using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvcContrib.SparkViewEngine.Parser.Markup;

namespace MvcContrib.SparkViewEngine.Compiler.NodeVisitors
{
	public class SpecialNodeInspector
	{
		private SpecialNode _node;
		public SpecialNodeInspector(SpecialNode node)
		{
			_node = node;
			Attributes = new List<AttributeNode>(node.Element.Attributes);
		}

		public string Name { get { return _node.Element.Name; } }
		public IList<AttributeNode> Attributes { get; set; }

		public AttributeNode TakeAttribute(string name)
		{
			if (name == null) throw new ArgumentNullException("name");
			var attr = Attributes.FirstOrDefault(a => a.Name == name);
			Attributes.Remove(attr);
			return attr;
		}
	}
}
