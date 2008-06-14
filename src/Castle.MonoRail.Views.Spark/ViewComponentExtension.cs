using System.Collections.Generic;
using System.Text;
using Spark;
using Spark.Compiler;
using Spark.Compiler.ChunkVisitors;
using Spark.Compiler.NodeVisitors;
using Spark.Parser.Markup;

namespace Castle.MonoRail.Views.Spark
{
	internal class ViewComponentExtension : ISparkExtension
	{
		private readonly ElementNode node;

		public ViewComponentExtension(ElementNode node)
		{
			this.node = node;
		}

		public void VisitNode(INodeVisitor visitor, IList<Node> body, IList<Chunk> chunks)
		{
			visitor.Accept(body);
		}

		public void VisitChunk(IChunkVisitor visitor, OutputLocation location, IList<Chunk> body, StringBuilder output)
		{
			if (location == OutputLocation.RenderMethod)
			{
				//todo: body? sections?
				output.AppendFormat("RenderComponent(\"{0}\", new System.Collections.Generic.Dictionary<string,object> {{", node.Name);

				var delimiter = "";
				foreach (var attribute in node.Attributes)
				{
					var code = attribute.Value.Replace("[[", "<").Replace("]]", ">");
					output.AppendFormat("{2}{{\"{0}\",{1}}}", attribute.Name, code, delimiter);
					delimiter = ", ";
				}
				output.AppendLine("}, output, new Action<System.Text.StringBuilder>(delegate(System.Text.StringBuilder output2) {");
				output.AppendLine("var output3 = output; output = output2;");
				visitor.Accept(body);
				output.AppendLine("output = output3;");
				output.AppendLine("}));");
			}
		}
	}
}