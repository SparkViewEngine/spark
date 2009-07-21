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
            var name = AttrName(attr);

            return name == "cache" || name == "cache.key" || name == "cache.expires";
        }

        private string AttrName(AttributeNode attr)
        {
            return Context.Namespaces == NamespacesType.Qualified
                       ? NameUtility.GetName(attr.Name)
                       : attr.Name;
        }

        protected override SpecialNode CreateWrappingNode(AttributeNode attr, ElementNode node)
        {
            var attrKey = 
                ExtractFakeAttribute(node, "cache", "key") ??
                ExtractFakeAttribute(node, "cache.key", "key");

            var attrExpires = 
                ExtractFakeAttribute(node, "cache.expires", "expires");

            var attrNodes = new[] { attrKey, attrExpires }.Where(x => x != null).ToList();

            var fakeElement = new ElementNode("cache", attrNodes, false) { OriginalNode = attr };
            return new SpecialNode(fakeElement);
        }

        private AttributeNode ExtractFakeAttribute(ElementNode node, string name, string fakeName)
        {
            var attribute = node.Attributes.SingleOrDefault(x => AttrName(x) == name);
            if (attribute == null)
                return null;

            node.Attributes.Remove(attribute);
            return new AttributeNode(fakeName, attribute.Nodes);
        }
    }
}
