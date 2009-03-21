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
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
	public class ForEachAttributeVisitor : NodeVisitor<ForEachAttributeVisitor.Frame>
	{
		public ForEachAttributeVisitor(VisitorContext context)
			: base(context)
		{
		}

		public class Frame
		{
			public string ClosingName { get; set; }
			public int ClosingNameOutstanding { get; set; }
		}

		bool IsEachAttribute(AttributeNode attr)
		{
			if (Context.Namespaces == NamespacesType.Unqualified)
				return attr.Name == "each";

			if (attr.Namespace != Constants.Namespace)
				return false;

			return NameUtility.GetName(attr.Name) == "each";
		}

		static SpecialNode CreateWrappingNode(AttributeNode eachAttr)
		{
			var fakeAttribute = new AttributeNode("each", eachAttr.Nodes) { OriginalNode = eachAttr };
			var fakeElement = new ElementNode("for", new[] { fakeAttribute }, false) { OriginalNode = eachAttr };
			return new SpecialNode(fakeElement);
		}

		protected override void Visit(ElementNode node)
		{
			var eachAttr = node.Attributes.FirstOrDefault(IsEachAttribute);
			if (eachAttr != null)
			{
				var wrapping = CreateWrappingNode(eachAttr);
				node.Attributes.Remove(eachAttr);
				wrapping.Body.Add(node);

				Nodes.Add(wrapping);
				if (!node.IsEmptyElement)
				{
					PushFrame(wrapping.Body, new Frame { ClosingName = node.Name, ClosingNameOutstanding = 1 });					
				}
			}
			else if (string.Equals(node.Name, FrameData.ClosingName) && !node.IsEmptyElement)
			{
				++FrameData.ClosingNameOutstanding;
				Nodes.Add(node);
			}
			else
			{
				Nodes.Add(node);
			}
		}

		protected override void Visit(EndElementNode node)
		{
			Nodes.Add(node);

			if (string.Equals(node.Name, FrameData.ClosingName))
			{
				--FrameData.ClosingNameOutstanding;
				if (FrameData.ClosingNameOutstanding == 0)
				{
					PopFrame();
				}
			}
		}

		protected override void Visit(SpecialNode node)
		{
			var reconstructed = new SpecialNode(node.Element);

			var nqName = NameUtility.GetName(node.Element.Name);

			AttributeNode eachAttr = null;
			if (nqName != "for")
				eachAttr = reconstructed.Element.Attributes.FirstOrDefault(IsEachAttribute);

			if (eachAttr != null)
			{
				reconstructed.Element.Attributes.Remove(eachAttr);

				var wrapping = CreateWrappingNode(eachAttr);
				Nodes.Add(wrapping);
				PushFrame(wrapping.Body, new Frame());
			}

			Nodes.Add(reconstructed);
			PushFrame(reconstructed.Body, new Frame());
			Accept(node.Body);
			PopFrame();

			if (eachAttr != null)
			{
				PopFrame();
			}
		}

		protected override void Visit(ExtensionNode node)
		{
			var reconstructed = new ExtensionNode(node.Element, node.Extension);

			var eachAttr = reconstructed.Element.Attributes.FirstOrDefault(IsEachAttribute);
			if (eachAttr != null)
			{
				reconstructed.Element.Attributes.Remove(eachAttr);

				var wrapping = CreateWrappingNode(eachAttr);
				Nodes.Add(wrapping);
				PushFrame(wrapping.Body, new Frame());
			}

			Nodes.Add(reconstructed);
			PushFrame(reconstructed.Body, new Frame());
			Accept(node.Body);
			PopFrame();

			if (eachAttr != null)
			{
				PopFrame();
			}
		}

	}
}
