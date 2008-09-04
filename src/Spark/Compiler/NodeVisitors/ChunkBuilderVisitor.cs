/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Spark.Compiler;
using Spark.Parser;
using Spark.Parser.Markup;
using Spark.Parser.Code;

namespace Spark.Compiler.NodeVisitors
{
    public class ChunkBuilderVisitor : AbstractNodeVisitor
    {
        private readonly IEnumerable<Paint> _paints;
        private readonly IDictionary<Node, Paint<Node>> _nodePaint;

        public IList<Chunk> Chunks { get; set; }
        private readonly IDictionary<string, Action<SpecialNode, SpecialNodeInspector>> _specialNodeMap;

        public ChunkBuilderVisitor(IEnumerable<Paint> paints)
        {
            _paints = paints;
            _nodePaint = _paints.OfType<Paint<Node>>().ToDictionary(paint => paint.Value);

            Chunks = new List<Chunk>();
            _specialNodeMap = new Dictionary<string, Action<SpecialNode, SpecialNodeInspector>>
                                  {
                                      {"var", VisitVar},
                                      {"def", VisitVar},
                                      {"global", (n,i)=>VisitGlobal(n)},
                                      {"viewdata", (n,i)=>VisitViewdata(i)},
                                      {"set", (n,i)=>VisitSet(i)},
                                      {"for", (n,i)=>VisitFor(n)},
                                      {"test", VisitIf},
                                      {"if", VisitIf},
                                      {"else", (n,i)=>VisitElse(i)},
                                      {"elseif", VisitElseIf},
                                      {"content", (n,i)=>VisitContent(i)},
                                      {"use", VisitUse},
                                      {"macro", (n,i)=>VisitMacro(i)},
                                      {"render", VisitRender}
                                  };
        }

        private Position Locate(Node expressionNode)
        {
            Paint<Node> paint;
            Node scan = expressionNode;
            while (scan != null)
            {
                if (_nodePaint.TryGetValue(scan, out paint))
                    return paint.Begin;
                scan = scan.OriginalNode;
            }
            return null;
        }

        public override IList<Node> Nodes
        {
            get { throw new NotImplementedException(); }
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
        private void AddKillingWhitespace(Chunk chunk)
        {
            var sendLiteral = Chunks.LastOrDefault() as SendLiteralChunk;
            if (sendLiteral != null && sendLiteral.Text.Trim() == string.Empty)
            {
                Chunks.Remove(sendLiteral);
            }
            Chunks.Add(chunk);
        }


        protected override void Visit(EntityNode entityNode)
        {
            AddLiteral("&" + entityNode.Name + ";");
        }

        protected override void Visit(ExpressionNode expressionNode)
        {
            Chunks.Add(new SendExpressionChunk { Code = expressionNode.Code, Position = Locate(expressionNode) });
        }



        protected override void Visit(StatementNode node)
        {
            AddKillingWhitespace(new CodeStatementChunk { Code = UnarmorCode(node.Code), Position = Locate(node) });
        }


        protected override void Visit(DoctypeNode docTypeNode)
        {
            //[28]   	doctypedecl	   ::=   	'<!DOCTYPE' S  Name (S  ExternalID)? S? ('[' intSubset ']' S?)? '>'
            //[75]   	ExternalID	   ::=   	'SYSTEM' S  SystemLiteral | 'PUBLIC' S PubidLiteral S SystemLiteral 
            //[12]   	PubidLiteral	   ::=   	'"' PubidChar* '"' | "'" (PubidChar - "'")* "'"
            //[11]   	SystemLiteral	   ::=   	('"' [^"]* '"') | ("'" [^']* "'")

            if (docTypeNode.ExternalId == null)
            {
                AddLiteral(string.Format("<!DOCTYPE {0}>",
                    docTypeNode.Name));
            }
            else if (docTypeNode.ExternalId.ExternalIdType == "SYSTEM")
            {
                char systemQuote = docTypeNode.ExternalId.SystemId.Contains("\"") ? '\'' : '\"';
                AddLiteral(string.Format("<!DOCTYPE {0} SYSTEM {2}{1}{2}>",
                    docTypeNode.Name, docTypeNode.ExternalId.SystemId, systemQuote));
            }
            else if (docTypeNode.ExternalId.ExternalIdType == "PUBLIC")
            {
                char systemQuote = docTypeNode.ExternalId.SystemId.Contains("\"") ? '\'' : '\"';
                AddLiteral(string.Format("<!DOCTYPE {0} PUBLIC \"{1}\" {3}{2}{3}>",
                    docTypeNode.Name, docTypeNode.ExternalId.PublicId, docTypeNode.ExternalId.SystemId, systemQuote));
            }
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
            var unconditionalNodes = new List<Node>();
            var conditionNodes = new List<ConditionNode>();
            foreach (var node in attributeNode.Nodes)
            {
                if (node is ConditionNode)
                {
                    // condition nodes take the prior unconditional nodes as content
                    var conditionNode = (ConditionNode)node;
                    conditionNodes.Add(conditionNode);
                    conditionNode.Nodes = unconditionalNodes;
                    unconditionalNodes = new List<Node>();
                }
                else
                {
                    // other types add to the unconditional list
                    unconditionalNodes.Add(node);
                }
            }

            if (unconditionalNodes.Count != 0)
            {
                // This attribute may not disapper - send it literally
                AddLiteral(" " + attributeNode.Name + "=\"");
                foreach (var node in conditionNodes)
                    Accept(node);
                foreach (var node in unconditionalNodes)
                    Accept(node);
                AddLiteral("\"");
            }
            else
            {
                var scope = new ScopeChunk();
                scope.Body.Add(new LocalVariableChunk { Name = "__just__once__", Value = "0" });

                _sendAttributeOnce = new ConditionalChunk { Condition = "__just__once__++ == 0", Type = ConditionalType.If };
                _sendAttributeOnce.Body.Add(new SendLiteralChunk { Text = " " + attributeNode.Name + "=\"" });

                var prior = Chunks;
                Chunks.Add(scope);
                Chunks = scope.Body;

                foreach (var node in conditionNodes)
                    Accept(node);

                Chunks = prior;
                _sendAttributeOnce = null;

                var ifWasSent = new ConditionalChunk { Condition = "__just__once__ != 0", Type = ConditionalType.If };
                scope.Body.Add(ifWasSent);
                ifWasSent.Body.Add(new SendLiteralChunk { Text = "\"" });
            }
        }

        private ConditionalChunk _sendAttributeOnce;

        protected override void Visit(ConditionNode conditionNode)
        {
            var conditionChunk = new ConditionalChunk() { Condition = conditionNode.Code, Type = ConditionalType.If, Position = Locate(conditionNode) };
            Chunks.Add(conditionChunk);

            if (_sendAttributeOnce != null)
                conditionChunk.Body.Add(_sendAttributeOnce);

            var prior = Chunks;
            Chunks = conditionChunk.Body;
            Accept(conditionNode.Nodes);
            Chunks = prior;
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
            if (!_specialNodeMap.ContainsKey(specialNode.Element.Name))
            {
                throw new CompilerException(string.Format("Unknown special node {0}", specialNode.Element.Name));
            }

            var prior = Chunks;
            try
            {
                var action = _specialNodeMap[specialNode.Element.Name];
                action(specialNode, new SpecialNodeInspector(specialNode));
            }
            finally
            {
                Chunks = prior;
            }
        }

        private void VisitMacro(SpecialNodeInspector inspector)
        {
            var name = inspector.TakeAttribute("name");
            var macro = new MacroChunk { Name = name.Value, Position = Locate(inspector.OriginalNode) };
            foreach (var attr in inspector.Attributes)
            {
                macro.Parameters.Add(new MacroParameter { Name = attr.Name, Type = attr.AsCode() });
            }
            AddUnordered(macro);
            Chunks = macro.Body;
            Accept(inspector.Body);
        }

        private void VisitUse(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            //TODO: change <use file=""> to <render partial="">, to avoid
            // random attribute conflicts on parameterized cases

            var content = inspector.TakeAttribute("content");
            var file = inspector.TakeAttribute("file");
            var namespaceAttr = inspector.TakeAttribute("namespace");
            var assemblyAttr = inspector.TakeAttribute("assembly");

            if (content != null)
            {
                var useContentChunk = new UseContentChunk { Name = content.Value, Position = Locate(inspector.OriginalNode) };
                Chunks.Add(useContentChunk);
                Chunks = useContentChunk.Default;
                Accept(specialNode.Body);
            }
            else if (file != null)
            {
                var scope = new ScopeChunk { Position = Locate(inspector.OriginalNode) };
                Chunks.Add(scope);
                Chunks = scope.Body;

                foreach (var attr in inspector.Attributes)
                {
                    Chunks.Add(new LocalVariableChunk { Name = attr.Name, Value = attr.AsCode(), Position = Locate(attr) });
                }

                var useFileChunk = new RenderPartialChunk { Name = file.Value, Position = Locate(inspector.OriginalNode) };
                Chunks.Add(useFileChunk);
                Chunks = useFileChunk.Body;
                Accept(inspector.Body);
            }
            else if (namespaceAttr != null || assemblyAttr != null)
            {
                if (namespaceAttr != null)
                {
                    var useNamespaceChunk = new UseNamespaceChunk { Namespace = namespaceAttr.Value };
                    AddUnordered(useNamespaceChunk);
                }
                if (assemblyAttr != null)
                {
                    var useAssemblyChunk = new UseAssemblyChunk { Assembly = assemblyAttr.Value };
                    AddUnordered(useAssemblyChunk);
                }
            }
            else
            {
                throw new CompilerException("Special node use had no understandable attributes");
            }
        }

        private void VisitRender(SpecialNode node, SpecialNodeInspector inspector)
        {
            var partial = inspector.TakeAttribute("partial");

            if (partial != null)
            {
                var scope = new ScopeChunk {Position = Locate(inspector.OriginalNode)};
                Chunks.Add(scope);
                Chunks = scope.Body;

                foreach (var attr in inspector.Attributes)
                {
                    Chunks.Add(new LocalVariableChunk { Name = attr.Name, Value = attr.AsCode(), Position = Locate(attr) });
                }

                var renderPartial = new RenderPartialChunk { Name = partial.Value, Position = Locate(inspector.OriginalNode) };
                Chunks.Add(renderPartial);

                Chunks = renderPartial.Body;
                Accept(inspector.Body);
            }
            else
            {
                var scope = new ScopeChunk { Position = Locate(inspector.OriginalNode) };
                Chunks.Add(scope);
                Chunks = scope.Body;

                foreach (var attr in inspector.Attributes)
                {
                    Chunks.Add(new LocalVariableChunk { Name = attr.Name, Value = attr.AsCode(), Position = Locate(attr) });
                }
                var render = new RenderSectionChunk();
                Chunks.Add(render);
            }
        }

        private void VisitContent(SpecialNodeInspector inspector)
        {
            var nameAttr = inspector.TakeAttribute("name");
            var varAttr = inspector.TakeAttribute("var");
            var defAttr = inspector.TakeAttribute("def");
            var setAttr = inspector.TakeAttribute("set");

            if (nameAttr != null)
            {
                var contentChunk = new ContentChunk { Name = nameAttr.Value, Position = Locate(inspector.OriginalNode) };
                Chunks.Add(contentChunk);
                Chunks = contentChunk.Body;
                Accept(inspector.Body);
            }
            else if (varAttr != null || defAttr != null)
            {
                var variableChunk = new LocalVariableChunk { Name = (varAttr ?? defAttr).AsCode(), Type = "string" };
                Chunks.Add(variableChunk);

                var contentSetChunk = new ContentSetChunk { Variable = variableChunk.Name, Position = Locate(inspector.OriginalNode) };
                Chunks.Add(contentSetChunk);
                Chunks = contentSetChunk.Body;
                Accept(inspector.Body);
            }
            else if (setAttr != null)
            {
                var addAttr = inspector.TakeAttribute("add");

                var contentSetChunk = new ContentSetChunk { Variable = setAttr.AsCode(), Position = Locate(inspector.OriginalNode) };

                if (addAttr != null)
                {
                    if (addAttr.Value == "before")
                        contentSetChunk.AddType = ContentAddType.InsertBefore;
                    else if (addAttr.Value == "after")
                        contentSetChunk.AddType = ContentAddType.AppendAfter;
                    else if (addAttr.Value == "replace")
                        contentSetChunk.AddType = ContentAddType.Replace;
                    else
                        throw new CompilerException("add attribute must be 'before', 'after', or 'replace");
                }

                Chunks.Add(contentSetChunk);
                Chunks = contentSetChunk.Body;
                Accept(inspector.Body);
            }
            else
            {
                throw new CompilerException("content element must have name, var, def, or set attribute");
            }
        }

        private void VisitIf(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            var conditionAttr = inspector.TakeAttribute("condition") ?? inspector.TakeAttribute("if");

            var ifChunk = new ConditionalChunk { Type = ConditionalType.If, Condition = conditionAttr.AsCode(), Position = Locate(inspector.OriginalNode) };
            Chunks.Add(ifChunk);
            Chunks = ifChunk.Body;
            Accept(specialNode.Body);
        }

        private void VisitElse(SpecialNodeInspector inspector)
        {
            if (!SatisfyElsePrecondition())
                throw new CompilerException("An 'else' may only follow an 'if' or 'elseif'.");

            var ifAttr = inspector.TakeAttribute("if");

            if (ifAttr == null)
            {
                var elseChunk = new ConditionalChunk { Type = ConditionalType.Else, Position = Locate(inspector.OriginalNode) };
                Chunks.Add(elseChunk);
                Chunks = elseChunk.Body;
            }
            else
            {
                var elseIfChunk = new ConditionalChunk { Type = ConditionalType.ElseIf, Condition = ifAttr.AsCode(), Position = Locate(inspector.OriginalNode) };
                Chunks.Add(elseIfChunk);
                Chunks = elseIfChunk.Body;
            }
            Accept(inspector.Body);
        }

        private void VisitElseIf(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            if (!SatisfyElsePrecondition())
                throw new CompilerException("An 'elseif' may only follow an 'if' or 'elseif'.");

            var conditionAttr = inspector.TakeAttribute("condition");
            var elseIfChunk = new ConditionalChunk { Type = ConditionalType.ElseIf, Condition = conditionAttr.AsCode(), Position = Locate(inspector.OriginalNode) };
            Chunks.Add(elseIfChunk);
            Chunks = elseIfChunk.Body;
            Accept(specialNode.Body);
        }

        private void VisitFor(SpecialNode specialNode)
        {
            var eachAttr = specialNode.Element.Attributes.FirstOrDefault(attr => attr.Name == "each");

            var forEachChunk = new ForEachChunk { Code = eachAttr.AsCode(), Position = Locate(specialNode.Element) };
            Chunks.Add(forEachChunk);
            Chunks = forEachChunk.Body;

            foreach (var attr in specialNode.Element.Attributes.Where(a => a != eachAttr))
            {
                Chunks.Add(new AssignVariableChunk { Name = attr.Name, Value = attr.AsCode(), Position = Locate(attr) });
            }

            Accept(specialNode.Body);
        }

        private void VisitSet(SpecialNodeInspector inspector)
        {
            foreach (var attr in inspector.Attributes)
            {
                Chunks.Add(new AssignVariableChunk { Name = attr.Name, Value = attr.AsCode(), Position = Locate(attr) });
            }
        }

        private void VisitViewdata(SpecialNodeInspector inspector)
        {
            var modelAttr = inspector.TakeAttribute("model");
            if (modelAttr != null)
                AddUnordered(new ViewDataModelChunk { TModel = modelAttr.AsCode() });

            foreach (var attr in inspector.Attributes)
            {
                string typeName = attr.AsCode();
                AddUnordered(new ViewDataChunk { Type = typeName, Name = attr.Name, Position = Locate(attr) });
            }
        }

        private void VisitGlobal(SpecialNode specialNode)
        {
            var typeAttr = specialNode.Element.Attributes.FirstOrDefault(attr => attr.Name == "type");
            string type = typeAttr != null ? typeAttr.AsCode() : "object";

            foreach (var attr in specialNode.Element.Attributes.Where(a => a != typeAttr))
            {
                AddUnordered(new GlobalVariableChunk { Type = type, Name = attr.Name, Value = attr.AsCode() });
            }
        }

        private void VisitVar(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            if (!specialNode.Element.IsEmptyElement)
            {
                var scope = new ScopeChunk();
                Chunks.Add(scope);
                Chunks = scope.Body;
            }

            var typeAttr = inspector.TakeAttribute("type");
            string type = typeAttr != null ? typeAttr.AsCode() : "var";

            foreach (var attr in inspector.Attributes)
            {
                Chunks.Add(new LocalVariableChunk { Type = type, Name = attr.Name, Value = attr.AsCode(), Position = Locate(attr) });   
            }

            Accept(specialNode.Body);
        }

        private bool SatisfyElsePrecondition()
        {
            var lastChunk = Chunks.LastOrDefault();

            // remove any literal that's entirely whitespace
            if (lastChunk is SendLiteralChunk)
            {
                var literal = ((SendLiteralChunk)lastChunk).Text;
                if (string.IsNullOrEmpty(literal.Trim()))
                {
                    Chunks.Remove(lastChunk);
                    lastChunk = Chunks.LastOrDefault();
                }
            }

            if (lastChunk is ConditionalChunk)
            {
                var conditionalType = ((ConditionalChunk)lastChunk).Type;
                if (conditionalType == ConditionalType.If ||
                    conditionalType == ConditionalType.ElseIf)
                {
                    return true;
                }
            }
            return false;
        }


        static string UnarmorCode(string code)
        {
            return code.Replace("[[", "<").Replace("]]", ">");
        }

        protected override void Visit(ExtensionNode extensionNode)
        {
            var extensionChunk = new ExtensionChunk { Extension = extensionNode.Extension };

            var prior = Chunks;
            try
            {
                Chunks.Add(extensionChunk);
                Chunks = extensionChunk.Body;
                extensionNode.Extension.VisitNode(this, extensionNode.Body, Chunks);
            }
            finally
            {
                Chunks = prior;
            }
        }



    }
}
