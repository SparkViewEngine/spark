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
	public class CacheAttributeVisitor : SpecialAttributeVisitorBase
    {
        public CacheAttributeVisitor(VisitorContext context)
			: base(context)
		{
		}

	    protected override bool IsSpecialAttribute(ElementNode element, AttributeNode attr)
        {
            var eltName = NameUtility.GetName(element.Name);
            if (eltName != "test" && eltName != "if" && eltName != "elseif" && eltName != "else")
                return false;

            if (Context.Namespaces == NamespacesType.Unqualified)
                return attr.Name == "once";

            if (attr.Namespace != Constants.Namespace)
                return false;

            var nqName = NameUtility.GetName(attr.Name);
            return nqName == "once";
        }

	    protected override SpecialNode CreateWrappingNode(AttributeNode conditionalAttr)
		{
            var fakeAttribute = new AttributeNode("once", conditionalAttr.Nodes);
            var fakeElement = new ElementNode("test", new[] { fakeAttribute }, false) { OriginalNode = conditionalAttr };
            return new SpecialNode(fakeElement);
        }
    }
}
