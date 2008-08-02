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
using Spark.Parser.Markup;
using Spark.Parser.Code;

namespace Spark.Compiler.NodeVisitors
{
    public class ChunkBuilderVisitor : AbstractNodeVisitor
    {
        public IList<Chunk> Chunks { get; set; }
        private IDictionary<string, Action<SpecialNode, SpecialNodeInspector>> _specialNodeMap;

        public ChunkBuilderVisitor()
        {
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
                                      {"content", (n,i)=>VisitContent(n)},
                                      {"use", VisitUse},
                                      {"macro", (n,i)=>VisitMacro(i)},
                                  };
        }

        public override IList<Node> Nodes
        {
            get { throw new System.NotImplementedException(); }
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
            Chunks.Add(new SendExpressionChunk { Code = expressionNode.Code });
        }


        protected override void Visit(StatementNode node)
        {
            AddKillingWhitespace(new CodeStatementChunk { Code = UnarmorCode(node.Code) });
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
            var macro = new MacroChunk { Name = name.Value };
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
                    Chunks.Add(new LocalVariableChunk { Name = attr.Name, Value = attr.AsCode() });
                }

                var useFileChunk = new RenderPartialChunk { Name = file.Value };
                Chunks.Add(useFileChunk);
            }
            else if (namespaceAttr != null || assemblyAttr != null)
            {
                if (namespaceAttr != null)
                {
                    var useNamespaceChunk = new UseNamespaceChunk {Namespace = namespaceAttr.Value};
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

        private void VisitContent(SpecialNode specialNode)
        {
            var nameAttr = specialNode.Element.Attributes.FirstOrDefault(attr => attr.Name == "name");

            var contentChunk = new ContentChunk { Name = nameAttr.Value };
            Chunks.Add(contentChunk);
            Chunks = contentChunk.Body;
            Accept(specialNode.Body);
        }

        private void VisitIf(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            var conditionAttr = inspector.TakeAttribute("condition") ?? inspector.TakeAttribute("if");

            var ifChunk = new ConditionalChunk { Type = ConditionalType.If, Condition = conditionAttr.AsCode() };
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
                var elseChunk = new ConditionalChunk { Type = ConditionalType.Else };
                Chunks.Add(elseChunk);
                Chunks = elseChunk.Body;

            }
            else
            {
                var elseIfChunk = new ConditionalChunk { Type = ConditionalType.ElseIf, Condition = ifAttr.AsCode() };
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
            var elseIfChunk = new ConditionalChunk { Type = ConditionalType.ElseIf, Condition = conditionAttr.AsCode() };
            Chunks.Add(elseIfChunk);
            Chunks = elseIfChunk.Body;
            Accept(specialNode.Body);
        }

        private void VisitFor(SpecialNode specialNode)
        {
            var eachAttr = specialNode.Element.Attributes.FirstOrDefault(attr => attr.Name == "each");

            var forEachChunk = new ForEachChunk { Code = eachAttr.AsCode() };
            Chunks.Add(forEachChunk);
            Chunks = forEachChunk.Body;

            foreach (var attr in specialNode.Element.Attributes.Where(a => a != eachAttr))
            {
                Chunks.Add(new AssignVariableChunk { Name = attr.Name, Value = attr.AsCode() });
            }

            Accept(specialNode.Body);
        }

        private void VisitSet(SpecialNodeInspector inspector)
        {
            foreach (var attr in inspector.Attributes)
            {
                Chunks.Add(new AssignVariableChunk { Name = attr.Name, Value = attr.AsCode() });
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
                AddUnordered(new ViewDataChunk { Type = typeName, Name = attr.Name });
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

            var typeAttr = inspector.TakeAttribute("var");
            string type = typeAttr != null ? typeAttr.AsCode() : "var";

            foreach (var attr in inspector.Attributes)
            {
                Chunks.Add(new LocalVariableChunk { Type = type, Name = attr.Name, Value = attr.AsCode() });
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
