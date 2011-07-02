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
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
	public class ForEachAttributeVisitor : SpecialAttributeVisitorBase
	{
		public ForEachAttributeVisitor(VisitorContext context)
			: base(context)
		{
		}

	    protected override bool IsSpecialAttribute(ElementNode element, AttributeNode attribute)
		{
	        var eltName = NameUtility.GetName(element.Name);
	        if (eltName == "for")
                return false;

			if (Context.Namespaces == NamespacesType.Unqualified)
				return attribute.Name == "each";

			if (attribute.Namespace != Constants.Namespace)
				return false;

			return NameUtility.GetName(attribute.Name) == "each";
		}

	    protected override SpecialNode CreateWrappingNode(AttributeNode attr, ElementNode node)
		{
			var fakeAttribute = new AttributeNode("each", '"', attr.Nodes) { OriginalNode = attr };
			var fakeElement = new ElementNode("for", new[] { fakeAttribute }, false) { OriginalNode = attr };
			return new SpecialNode(fakeElement);
		}
	}
}
