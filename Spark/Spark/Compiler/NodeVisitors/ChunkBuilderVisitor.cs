using System.Collections.Generic;
using System.Linq;
using MvcContrib.SparkViewEngine.Compiler;
using MvcContrib.SparkViewEngine.Parser.Markup;

namespace MvcContrib.SparkViewEngine.Compiler.NodeVisitors
{
	public class ChunkBuilderVisitor : NodeVisitor
	{
		public IList<Chunk> Chunks { get; set; }

		public ChunkBuilderVisitor()
		{
			Chunks = new List<Chunk>();
		}

		protected override void Visit(TextNode textNode)
		{
			AddLiteral(textNode.Text);
		}

		private void AddLiteral(string text)
		{
			var sendLiteral = Chunks.LastOrDefault() as SendLiteralChunk;
			if (sendLiteral == null)
			{
				sendLiteral = new SendLiteralChunk { Text = text };
				Chunks.Add(sendLiteral);
			}
			else
			{
				sendLiteral.Text += text;
			}
		}

		private void AddUnordered(Chunk chunk)
		{
			var sendLiteral = Chunks.LastOrDefault() as SendLiteralChunk;
			if (sendLiteral == null)
			{
				Chunks.Add(chunk);
			}
			else
			{
				Chunks.Insert(Chunks.Count - 1, chunk);
			}
		}

		protected override void Visit(EntityNode entityNode)
		{
			AddLiteral("&" + entityNode.Name + ";");
		}

		protected override void Visit(ExpressionNode expressionNode)
		{
			Chunks.Add(new SendExpressionChunk { Code = expressionNode.Code.Replace("[[", "<").Replace("]]", ">") });
		}

		protected override void Visit(DoctypeNode docTypeNode)
		{
			//TODO: repeat from source instead of rebuilding?
			AddLiteral("<!--doctype-->");
		}

		protected override void Visit(ElementNode elementNode)
		{
			AddLiteral("<" + elementNode.Name);
			foreach (var attribute in elementNode.Attributes)
				Accept(attribute);
			AddLiteral(elementNode.IsEmptyElement ? "/>" : ">");
		}

		protected override void Visit(AttributeNode attributeNode)
		{
			AddLiteral(" " + attributeNode.Name + "=\"");
			foreach (var node in attributeNode.Nodes)
				Accept(node);
			AddLiteral("\"");
		}

		protected override void Visit(EndElementNode endElementNode)
		{
			AddLiteral("</" + endElementNode.Name + ">");
		}


		protected override void Visit(CommentNode commentNode)
		{
			AddLiteral("<!--" + commentNode.Text + "-->");
		}

		protected override void Visit(SpecialNode specialNode)
		{
			var prior = Chunks;
			try
			{
				var inspector = new SpecialNodeInspector(specialNode);
				switch (inspector.Name)
				{
					case "var":
						{
							if (!specialNode.Element.IsEmptyElement)
							{
								var scope = new ScopeChunk();
								Chunks.Add(scope);
								Chunks = scope.Body;
							}

							var typeAttr = specialNode.Element.Attributes.FirstOrDefault(attr => attr.Name == "type");
							string type = typeAttr != null ? typeAttr.Value : "var";

							foreach (var attr in specialNode.Element.Attributes.Where(a => a != typeAttr))
							{
								Chunks.Add(new LocalVariableChunk { Type = type.Replace("[[", "<").Replace("]]", ">"), Name = attr.Name, Value = attr.Value.Replace("[[", "<").Replace("]]", ">") });
							}

							Accept(specialNode.Body);
						}
						break;
					case "global":
						{
							var typeAttr = specialNode.Element.Attributes.FirstOrDefault(attr => attr.Name == "type");
							string type = typeAttr != null ? typeAttr.Value : "object";

							foreach (var attr in specialNode.Element.Attributes.Where(a => a != typeAttr))
							{
								AddUnordered(new GlobalVariableChunk { Type = type.Replace("[[", "<").Replace("]]", ">"), Name = attr.Name, Value = attr.Value.Replace("[[", "<").Replace("]]", ">") });
							}
						}
						break;
					case "viewdata":
						{
							var modelAttr = inspector.TakeAttribute("model");
							if (modelAttr != null)
								AddUnordered(new ViewDataModelChunk { TModel = modelAttr.Value });

							foreach (var attr in inspector.Attributes)
							{
								string typeName = attr.Value.Replace("[[", "<").Replace("]]", ">");
								AddUnordered(new ViewDataChunk { Type = typeName, Name = attr.Name });
							}
						}
						break;
					case "set":
						{
							var typeAttr = specialNode.Element.Attributes.FirstOrDefault(attr => attr.Name == "type");
							string type = typeAttr != null ? typeAttr.Value : "object";

							foreach (var attr in specialNode.Element.Attributes.Where(a => a != typeAttr))
							{
								Chunks.Add(new AssignVariableChunk { Name = attr.Name, Value = attr.Value });
							}
						}
						break;
					case "for":
						{
							var eachAttr = specialNode.Element.Attributes.FirstOrDefault(attr => attr.Name == "each");

							var forEachChunk = new ForEachChunk { Code = eachAttr.Value };
							Chunks.Add(forEachChunk);
							Chunks = forEachChunk.Body;

							foreach (var attr in specialNode.Element.Attributes.Where(a => a != eachAttr))
							{
								Chunks.Add(new AssignVariableChunk { Name = attr.Name, Value = attr.Value });
							}

							Accept(specialNode.Body);
						}
						break;
					case "content":
						{
							var nameAttr = specialNode.Element.Attributes.FirstOrDefault(attr => attr.Name == "name");

							var contentChunk = new ContentChunk { Name = nameAttr.Value };
							Chunks.Add(contentChunk);
							Chunks = contentChunk.Body;
							Accept(specialNode.Body);
						}
						break;
					case "use":
						{
							//TODO: change <use file=""> to <render partial="">, to avoid
							// random attribute conflicts on parameterized cases

							var content = inspector.TakeAttribute("content");
							var file = inspector.TakeAttribute("file");
							var namespaceAttr = inspector.TakeAttribute("namespace");
							if (content != null)
							{
								var useContentChunk = new UseContentChunk { Name = content.Value };
								Chunks.Add(useContentChunk);
								Chunks = useContentChunk.Default;
								Accept(specialNode.Body);
							}
							else if (file != null)
							{
								var scope = new ScopeChunk();
								Chunks.Add(scope);
								Chunks = scope.Body;

								foreach (var attr in inspector.Attributes)
								{
									Chunks.Add(new LocalVariableChunk { Name = attr.Name, Value = attr.Value });
								}

								var useFileChunk = new RenderPartialChunk { Name = file.Value };
								Chunks.Add(useFileChunk);
							}
							else if (namespaceAttr != null)
							{
								var useNamespaceChunk = new UseNamespaceChunk { Namespace = namespaceAttr.Value };
								AddUnordered(useNamespaceChunk);
							}
							else
							{
								throw new CompilerException("Special node use had no understandable attributes");
							}
						}
						break;
					default:
						throw new CompilerException(string.Format("Unknown special node {0}", specialNode.Element.Name));
				}
			}
			finally
			{
				Chunks = prior;
			}
		}
	}
}
