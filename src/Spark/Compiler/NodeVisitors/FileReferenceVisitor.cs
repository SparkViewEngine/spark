using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvcContrib.SparkViewEngine.Parser.Markup;

namespace MvcContrib.SparkViewEngine.Compiler.NodeVisitors
{
	public class FileReference
	{
		public string File { get; set; }
		public SpecialNode UseFileNode { get; set; }
	}

	public class FileReferenceVisitor : NodeVisitor
	{
		private IList<FileReference> _references = new List<FileReference>();

		public IList<FileReference> References
		{
			get { return _references; }
		}

		protected override void Visit(EndElementNode endElementNode)
		{
		}

		protected override void Visit(ElementNode elementNode)
		{
		}

		protected override void Visit(SpecialNode specialNode)
		{
			var inspector = new SpecialNodeInspector(specialNode);

			if (inspector.Name == "use")
			{
				var file = inspector.TakeAttribute("file");
				if (file != null)
				{
					References.Add(new FileReference { File = file.Value, UseFileNode = specialNode });
				}
			}

			Accept(specialNode.Body);
		}

		protected override void Visit(AttributeNode attributeNode)
		{
		}

		protected override void Visit(EntityNode entityNode)
		{
		}

		protected override void Visit(ExpressionNode expressionNode)
		{
		}

		protected override void Visit(TextNode textNode)
		{
		}

		protected override void Visit(DoctypeNode docTypeNode)
		{
		}

		protected override void Visit(CommentNode commentNode)
		{
		}
	}
}
