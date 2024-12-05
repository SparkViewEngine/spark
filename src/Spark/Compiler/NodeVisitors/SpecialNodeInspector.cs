// Copyright 2008-2024 Louis DeJardin
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
using System;
using System.Collections.Generic;
using System.Linq;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
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
        public bool IsEmptyElement { get { return _node.Element.IsEmptyElement; } }
        public IList<Node> Body { get { return _node.Body; } }

        public AttributeNode TakeAttribute(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            var attr = Attributes.FirstOrDefault(a => a.Name == name);
            Attributes.Remove(attr);
            return attr;
        }

        public AttributeNode TakeAttribute(string name, NamespacesType nsType)
        {
            AttributeNode attr;
            if (nsType == NamespacesType.Unqualified)
            {
                attr = Attributes.FirstOrDefault(a => a.Name == name);
            }
            else
            {
                attr = Attributes.FirstOrDefault(a =>
                    (_node.Element.Namespace == Constants.Namespace && a.Name == name) ||
                    (a.Namespace == Constants.Namespace && NameUtility.GetName(a.Name) == name));
            }

            Attributes.Remove(attr);
            return attr;
        }

        public ElementNode OriginalNode { get { return _node.Element; } }
    }
}
