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
    public class UrlAttributeVisitor : NodeVisitor
    {
        private readonly IList<ElementSpecs> _specs;

        public UrlAttributeVisitor(VisitorContext context) : base(context)
        {
            _specs = new[] 
            {
                new ElementSpecs("a","href"),
                new ElementSpecs("applet", "codebase"),
                new ElementSpecs("area", "href"),
                new ElementSpecs("base", "href"),
                new ElementSpecs("blockquote", "cite"),
                new ElementSpecs("body", "background"),
                new ElementSpecs("del","cite"),
                new ElementSpecs("form", "action"),
                new ElementSpecs("frame", "longdesc", "src"),
                new ElementSpecs("head", "profile"),
                new ElementSpecs("iframe", "longdesc", "src"),
                new ElementSpecs("img", "longdesc", "src", "usemap"),
                new ElementSpecs("input", "src", "usemap"),
                new ElementSpecs("ins", "cite"),
                new ElementSpecs("link","href"),
                new ElementSpecs("object", "classid", "codebase", "data", "usemap"),
                new ElementSpecs("script", "src"),
                new ElementSpecs("q", "cite") 
            };
        }



        class ElementSpecs
        {
            private readonly string name;
            private readonly IList<string> attributes;

            public ElementSpecs(string name, params string[] attributes)
            {
                this.name = name;
                this.attributes = attributes;
            }

            public IList<string> Attributes
            {
                get { return attributes; }
            }

            public string Name
            {
                get { return name; }
            }
        }

        protected override void Visit(ElementNode node)
        {
            Process(node);
            base.Visit(node);
        }

        private void Process(ElementNode element)
        {
            var elementSpec =
                _specs.FirstOrDefault(
                    m => string.Equals(m.Name, element.Name, StringComparison.InvariantCultureIgnoreCase));

            if (elementSpec == null)
                return;

            foreach (var attribute in element.Attributes)
            {
                var attrName = attribute.Name;
                if (elementSpec.Attributes.Any(n => string.Equals(n, attrName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Process(attribute);
                }
            }
        }

        private void Process(AttributeNode attribute)
        {
            var first = attribute.Nodes.FirstOrDefault() as TextNode;
            if (first == null || !first.Text.StartsWith("~/"))
                return;

            var expr = "SiteResource(\"" + first.Text + "\")";
            attribute.Nodes[0] = new ExpressionNode(expr) { OriginalNode = first };

            
        }

        
    }
}
