//-------------------------------------------------------------------------
// <copyright file="Node.cs">
// Copyright 2008-2010 Louis DeJardin - http://whereslou.com
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
// </copyright>
// <author>John Gietzen</author>
//-------------------------------------------------------------------------

namespace Spark.Compiler.NodeVisitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Spark.Parser;
    using Spark.Parser.Code;
    using Spark.Parser.Markup;

    public class WhitespaceCleanerVisitor : NodeVisitor<WhitespaceCleanerVisitor.Frame>
    {
        private readonly IDictionary<Node, Paint<Node>> nodePaint;
        private readonly IList<Node> nodes = new List<Node>();
        private readonly IDictionary<string, Action<SpecialNode, SpecialNodeInspector>> specialNodeMap;

        public WhitespaceCleanerVisitor(VisitorContext context)
            : base(context)
        {
            this.nodePaint = Context.Paint.OfType<Paint<Node>>().ToDictionary(paint => paint.Value);

            this.specialNodeMap = new Dictionary<string, Action<SpecialNode, SpecialNodeInspector>>
                                  {
                                      {"for", VisitFor},
                                      {"test", VisitIf},
                                      {"if", VisitIf},
                                      {"else", VisitIf},
                                      {"elseif", VisitIf},
                                  };
        }

        protected void VisitIf(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            if (!FirstChildBeginsWithNewline(specialNode))
            {
                var newBody = new List<Node>();
                newBody.Add(new TextNode(specialNode.Element.PreceedingWhitespace));
                if (specialNode.Body != null)
                {
                    newBody.AddRange(specialNode.Body);
                }

                specialNode.Body = newBody;
            }

            specialNode.Element.PreceedingWhitespace = string.Empty;
        }

        protected void VisitFor(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            if (!FirstChildBeginsWithNewline(specialNode))
            {
                var eachAttr = inspector.TakeAttribute("each");
                var eachInspector = new ForEachInspector(AsCode(eachAttr));

                var code = eachInspector.Recognized ? eachInspector.VariableName + "IsFirst" : "Once(\"" + Guid.NewGuid() + "\")";

                var newNode = new SpecialNode(new ElementNode("if", new[] { new AttributeNode("condition", code) }, false))
                {
                    Body = new [] {
                        new TextNode(specialNode.Element.PreceedingWhitespace),
                    },
                };

                var newBody = new List<Node>();
                newBody.Add(newNode);
                if (specialNode.Body != null)
                {
                    newBody.AddRange(specialNode.Body);
                }

                specialNode.Body = newBody;
            }

            specialNode.Element.PreceedingWhitespace = string.Empty;
        }

        private bool FirstChildBeginsWithNewline(SpecialNode node)
        {
            if (node == null ||
                node.Body == null ||
                node.Body.Count == 0)
            {
                return false;
            }

            var firstChild = node.Body[0];

            if (firstChild == null)
            {
                return false;
            }

            var textNode = firstChild as TextNode;
            if (textNode != null)
            {
                return textNode.Text.IndexOf("\r\n") == 0 ||
                    textNode.Text.IndexOf("\n") == 0;
            }

            var elementNode = firstChild as ElementNode;
            if (elementNode == null)
            {
                var specialNode = firstChild as SpecialNode;
                if (specialNode != null)
                {
                    elementNode = specialNode.Element;
                }
            }

            if (elementNode != null)
            {
                return elementNode.PreceedingWhitespace.IndexOf("\r\n") == 0 ||
                    elementNode.PreceedingWhitespace.IndexOf("\n") == 0;
            }

            return false;
        }

        private Snippets AsCode(AttributeNode attr)
        {
            var begin = Locate(attr.Nodes.FirstOrDefault());
            var end = LocateEnd(attr.Nodes.LastOrDefault());
            if (begin == null || end == null)
            {
                begin = new Position(new SourceContext(attr.Value));
                end = begin.Advance(begin.PotentialLength());
            }
            return Context.SyntaxProvider.ParseFragment(begin, end);
        }

        private Position Locate(Node expressionNode)
        {
            Paint<Node> paint;
            Node scan = expressionNode;
            while (scan != null)
            {
                if (this.nodePaint.TryGetValue(scan, out paint))
                    return paint.Begin;
                scan = scan.OriginalNode;
            }
            return null;
        }

        private Position LocateEnd(Node expressionNode)
        {
            Paint<Node> paint;
            Node scan = expressionNode;
            while (scan != null)
            {
                if (this.nodePaint.TryGetValue(scan, out paint))
                    return paint.End;
                scan = scan.OriginalNode;
            }
            return null;
        }

        protected override void Visit(SpecialNode specialNode)
        {
            string name = NameUtility.GetName(specialNode.Element.Name);

            if (!string.IsNullOrEmpty(specialNode.Element.PreceedingWhitespace) &&
                this.specialNodeMap.ContainsKey(name))
            {
                var action = this.specialNodeMap[name];
                action(specialNode, new SpecialNodeInspector(specialNode));
            }

            base.Visit(specialNode);
        }

        public class Frame
        {
        }
    }
}
