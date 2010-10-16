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
using System;
using System.Collections.Generic;
using System.Linq;
using Spark.Parser;
using Spark.Parser.Markup;
using Spark.Parser.Code;

namespace Spark.Compiler.NodeVisitors
{
    public class ChunkBuilderVisitor : AbstractNodeVisitor
    {
        private readonly IDictionary<Node, Paint<Node>> _nodePaint;
        private readonly IDictionary<string, Action<SpecialNode, SpecialNodeInspector>> _specialNodeMap;

        public IList<Chunk> Chunks { get; set; }

        private IDictionary<string, IList<Chunk>> SectionChunks { get; set; }


        public IDictionary<string, Action<SpecialNode, SpecialNodeInspector>> SpecialNodeMap
        {
            get
            {
                return _specialNodeMap;
            }
        }
        class Frame : IDisposable
        {
            private readonly ChunkBuilderVisitor _visitor;
            private readonly IList<Chunk> _chunks;
            private readonly IDictionary<string, IList<Chunk>> _sectionChunks;

            public Frame(ChunkBuilderVisitor visitor,
                IList<Chunk> chunks)
                : this(visitor, chunks, null)
            {
            }

            public Frame(ChunkBuilderVisitor visitor,
                IList<Chunk> chunks,
                IDictionary<string, IList<Chunk>> sectionChunks)
            {
                _visitor = visitor;

                _chunks = _visitor.Chunks;
                _sectionChunks = _visitor.SectionChunks;

                _visitor.Chunks = chunks;
                _visitor.SectionChunks = sectionChunks;
            }

            public void Dispose()
            {
                _visitor.Chunks = _chunks;
                _visitor.SectionChunks = _sectionChunks;
            }
        }

        public ChunkBuilderVisitor(VisitorContext context)
            : base(context)
        {
            _nodePaint = Context.Paint.OfType<Paint<Node>>().ToDictionary(paint => paint.Value);

            Chunks = new List<Chunk>();
            _specialNodeMap = new Dictionary<string, Action<SpecialNode, SpecialNodeInspector>>
                                  {
                                      {"var", VisitVar},
                                      {"def", VisitVar},
                                      {"default", VisitDefault},
                                      {"global", (n,i)=>VisitGlobal(n)},
                                      {"viewdata", (n,i)=>VisitViewdata(i)},
                                      {"set", (n,i)=>VisitSet(i)},
                                      {"for", VisitFor},
                                      {"test", VisitIf},
                                      {"if", VisitIf},
                                      {"else", (n,i)=>VisitElse(i)},
                                      {"elseif", VisitElseIf},
                                      {"content", (n,i)=>VisitContent(i)},
                                      {"use", VisitUse},
                                      {"macro", (n,i)=>VisitMacro(i)},
                                      {"render", VisitRender},
                                      {"section", VisitSection},
                                      {"cache", VisitCache},
                                      {"markdown", VisitMarkdown}
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

        private Position LocateEnd(Node expressionNode)
        {
            Paint<Node> paint;
            Node scan = expressionNode;
            while (scan != null)
            {
                if (_nodePaint.TryGetValue(scan, out paint))
                    return paint.End;
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

        protected override void Visit(ExpressionNode node)
        {
            Chunks.Add(new SendExpressionChunk
            {
                Code = node.Code,
                Position = Locate(node),
                SilentNulls = node.SilentNulls,
                AutomaticallyEncode = node.AutomaticEncoding
            });
        }



        protected override void Visit(StatementNode node)
        {
            //REFACTOR: what is UnarmorCode doing at this point?
            AddKillingWhitespace(new CodeStatementChunk { Code = node.Code, Position = Locate(node) });
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

        protected override void Visit(XMLDeclNode node)
        {
            //[23]   	XMLDecl	   ::=   	'<?xml' VersionInfo  EncodingDecl? SDDecl? S? '?>'
            //[24]   	VersionInfo	   ::=   	 S 'version' Eq ("'" VersionNum "'" | '"' VersionNum '"')
            //[80]   	EncodingDecl	   ::=   	 S 'encoding' Eq ('"' EncName '"' | "'" EncName "'" ) 
            //[32]   	SDDecl	   ::=   	 S 'standalone' Eq (("'" ('yes' | 'no') "'") | ('"' ('yes' | 'no') '"')) 

            var encoding = "";
            if (!string.IsNullOrEmpty(node.Encoding))
            {
                if (node.Encoding.Contains("\""))
                    encoding = string.Concat(" encoding='", node.Encoding, "'");
                else
                    encoding = string.Concat(" encoding=\"", node.Encoding, "\"");
            }

            var standalone = "";
            if (!string.IsNullOrEmpty(node.Standalone))
                standalone = string.Concat(" standalone=\"", node.Standalone, "\"");

            AddLiteral(string.Concat("<?xml version=\"1.0\"", encoding, standalone, " ?>"));
        }

        protected override void Visit(ProcessingInstructionNode node)
        {
            //[16]   	PI	   ::=   	'<?' PITarget (S (Char* - (Char* '?>' Char*)))? '?>'
            if (string.IsNullOrEmpty(node.Body))
                AddLiteral(string.Concat("<?", node.Name, "?>"));
            else
                AddLiteral(string.Concat("<?", node.Name, " ", node.Body, "?>"));
        }

        protected override void Visit(ElementNode node)
        {
            AddLiteral("<" + node.Name);

            foreach (var attribute in node.Attributes)
                Accept(attribute);

            AddLiteral(node.IsEmptyElement ? "/>" : ">");
        }

        protected override void Visit(AttributeNode attributeNode)
        {
            var accumulatedNodes = new List<Node>();
            var processedNodes = new List<Node>();
            foreach (var node in attributeNode.Nodes)
            {
                if (node is ConditionNode)
                {
                    // condition nodes take the prior unconditional nodes as content
                    var conditionNode = (ConditionNode)node;
                    MovePriorNodesUnderCondition(conditionNode, accumulatedNodes);

                    // prior nodes and condition are set for output
                    processedNodes.AddRange(accumulatedNodes);
                    processedNodes.Add(conditionNode);

                    accumulatedNodes.Clear();
                }
                else
                {
                    // other types add to the unconditional list
                    accumulatedNodes.Add(node);
                }
            }
            processedNodes.AddRange(accumulatedNodes);

            var allNodesAreConditional = processedNodes.All(node => node is ConditionNode);

            if (allNodesAreConditional == false || processedNodes.Any() == false)
            {
                // This attribute may not disapper - send it literally
                AddLiteral(" " + attributeNode.Name + "=\"");
                foreach (var node in processedNodes)
                    Accept(node);
                AddLiteral("\"");
            }
            else
            {
                var scope = new ScopeChunk();
                scope.Body.Add(new LocalVariableChunk { Name = "__just__once__", Value = new Snippets("0") });

                _sendAttributeOnce = new ConditionalChunk { Type = ConditionalType.If, Condition = new Snippets("__just__once__ < 1") };
                _sendAttributeOnce.Body.Add(new SendLiteralChunk { Text = " " + attributeNode.Name + "=\"" });
                _sendAttributeIncrement = new AssignVariableChunk { Name = "__just__once__", Value = "1" };


                Chunks.Add(scope);

                using (new Frame(this, scope.Body))
                {
                    foreach (var node in processedNodes)
                        Accept(node);
                }
                _sendAttributeOnce = null;
                _sendAttributeIncrement = null;

                var ifWasSent = new ConditionalChunk { Type = ConditionalType.If, Condition = new Snippets("__just__once__ > 0") };
                scope.Body.Add(ifWasSent);
                ifWasSent.Body.Add(new SendLiteralChunk { Text = "\"" });
            }
        }

        private static void MovePriorNodesUnderCondition(ConditionNode condition, ICollection<Node> priorNodes)
        {
            while (priorNodes.Count != 0)
            {
                var priorNode = priorNodes.Last();
                priorNodes.Remove(priorNode);
                if (!(priorNode is TextNode))
                {
                    condition.Nodes.Insert(0, priorNode);
                    continue;
                }

                // for text, extend back to and include the last whitespace
                var priorText = ((TextNode)priorNode).Text;
                var finalPieceIndex = priorText.LastIndexOfAny(new[] { ' ', '\t', '\r', '\n' }) + 1;
                if (finalPieceIndex == 0)
                {
                    condition.Nodes.Insert(0, priorNode);
                    continue;
                }

                while (finalPieceIndex != 0 && char.IsWhiteSpace(priorText[finalPieceIndex - 1]))
                    --finalPieceIndex;

                condition.Nodes.Insert(0, new TextNode(priorText.Substring(finalPieceIndex)) { OriginalNode = priorNode });
                if (finalPieceIndex != 0)
                {
                    priorNodes.Add(new TextNode(priorText.Substring(0, finalPieceIndex)) { OriginalNode = priorNode });
                }
                return;
            }
        }

        private ConditionalChunk _sendAttributeOnce;
        private Chunk _sendAttributeIncrement;

        protected override void Visit(ConditionNode conditionNode)
        {
            var conditionChunk = new ConditionalChunk
                                     {
                                         Condition = conditionNode.Code,
                                         Type = ConditionalType.If,
                                         Position = Locate(conditionNode)                                         
                                     };
            Chunks.Add(conditionChunk);

            if (_sendAttributeOnce != null)
                conditionChunk.Body.Add(_sendAttributeOnce);
            if (_sendAttributeIncrement != null)
                conditionChunk.Body.Add(_sendAttributeIncrement);

            using (new Frame(this, conditionChunk.Body))
            {
                Accept(conditionNode.Nodes);
            }
        }

        protected override void Visit(EndElementNode node)
        {
            AddLiteral("</" + node.Name + ">");
        }


        protected override void Visit(CommentNode commentNode)
        {
            AddLiteral("<!--" + commentNode.Text + "-->");
        }

        protected override void Visit(SpecialNode specialNode)
        {
            string nqName = NameUtility.GetName(specialNode.Element.Name);
            if (!SpecialNodeMap.ContainsKey(nqName))
            {
                throw new CompilerException(string.Format("Unknown special node {0}", specialNode.Element.Name));
            }


            var action = SpecialNodeMap[nqName];
            action(specialNode, new SpecialNodeInspector(specialNode));
        }

        Snippets AsCode(AttributeNode attr)
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

        private void VisitMacro(SpecialNodeInspector inspector)
        {
            var name = inspector.TakeAttribute("name");
            var macro = new MacroChunk { Name = name.Value, Position = Locate(inspector.OriginalNode) };
            foreach (var attr in inspector.Attributes)
            {
                macro.Parameters.Add(new MacroParameter { Name = attr.Name, Type = AsCode(attr) });
            }
            AddUnordered(macro);
            using (new Frame(this, macro.Body))
            {
                Accept(inspector.Body);
            }
        }

        private void VisitUse(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            var file = inspector.TakeAttribute("file");
            if (file != null)
            {
                var scope = new ScopeChunk { Position = Locate(inspector.OriginalNode) };
                Chunks.Add(scope);
                using (new Frame(this, scope.Body))
                {
                    foreach (var attr in inspector.Attributes)
                    {
                        Chunks.Add(new LocalVariableChunk { Name = attr.Name, Value = AsCode(attr), Position = Locate(attr) });
                    }

                    var useFileChunk = new RenderPartialChunk { Name = file.Value, Position = Locate(inspector.OriginalNode) };
                    Chunks.Add(useFileChunk);
                    using (new Frame(this, useFileChunk.Body, useFileChunk.Sections))
                    {
                        Accept(inspector.Body);
                    }
                }
            }
            else
            {
                var contentAttr = inspector.TakeAttribute("content");
                var namespaceAttr = inspector.TakeAttribute("namespace");
                var assemblyAttr = inspector.TakeAttribute("assembly");
                var importAttr = inspector.TakeAttribute("import");
                var masterAttr = inspector.TakeAttribute("master");
                var pageBaseTypeAttr = inspector.TakeAttribute("pageBaseType");

                if (contentAttr != null)
                {
                    var useContentChunk = new UseContentChunk { Name = contentAttr.Value, Position = Locate(inspector.OriginalNode) };
                    Chunks.Add(useContentChunk);
                    using (new Frame(this, useContentChunk.Default))
                    {
                        Accept(specialNode.Body);
                    }
                }
                else if (namespaceAttr != null || assemblyAttr != null)
                {
                    if (namespaceAttr != null)
                    {
                        var useNamespaceChunk = new UseNamespaceChunk { Namespace = AsCode(namespaceAttr) };
                        AddUnordered(useNamespaceChunk);
                    }
                    if (assemblyAttr != null)
                    {
                        var useAssemblyChunk = new UseAssemblyChunk { Assembly = assemblyAttr.Value };
                        AddUnordered(useAssemblyChunk);
                    }
                }
                else if (importAttr != null)
                {
                    var useImportChunk = new UseImportChunk { Name = importAttr.Value };
                    AddUnordered(useImportChunk);
                }
                else if (masterAttr != null)
                {
                    var useMasterChunk = new UseMasterChunk { Name = masterAttr.Value };
                    AddUnordered(useMasterChunk);
                }
                else if (pageBaseTypeAttr != null)
                {
                    var usePageBaseTypeChunk = new PageBaseTypeChunk { BaseClass = AsCode(pageBaseTypeAttr) };
                    AddUnordered(usePageBaseTypeChunk);
                }
                else
                {
                    throw new CompilerException("Special node use had no understandable attributes");
                }
            }
        }

        private void VisitRender(SpecialNode node, SpecialNodeInspector inspector)
        {
            var partial = inspector.TakeAttribute("partial");

            if (partial != null)
            {
                var scope = new ScopeChunk { Position = Locate(inspector.OriginalNode) };
                Chunks.Add(scope);
                using (new Frame(this, scope.Body))
                {
                    foreach (var attr in inspector.Attributes)
                    {
                        Chunks.Add(new LocalVariableChunk { Name = attr.Name, Value = AsCode(attr), Position = Locate(attr) });
                    }

                    var renderPartial = new RenderPartialChunk { Name = partial.Value, Position = Locate(inspector.OriginalNode) };
                    Chunks.Add(renderPartial);

                    using (new Frame(this, renderPartial.Body, renderPartial.Sections))
                    {
                        Accept(inspector.Body);
                    }
                }
            }
            else
            {
                var sectionAttr = inspector.TakeAttribute("section");

                string sectionName = null;
                if (sectionAttr != null)
                    sectionName = sectionAttr.Value;

                var scope = new ScopeChunk { Position = Locate(inspector.OriginalNode) };
                Chunks.Add(scope);
                using (new Frame(this, scope.Body))
                {
                    foreach (var attr in inspector.Attributes)
                    {
                        Chunks.Add(new LocalVariableChunk { Name = attr.Name, Value = AsCode(attr), Position = Locate(attr) });
                    }
                    var render = new RenderSectionChunk { Name = sectionName };
                    Chunks.Add(render);
                    using (new Frame(this, render.Default))
                    {
                        Accept(inspector.Body);
                    }
                }
            }
        }

        private void VisitSection(SpecialNode node, SpecialNodeInspector inspector)
        {
            if (SectionChunks == null)
                throw new CompilerException("Section cannot be used at this location", Locate(node.Element));

            var name = inspector.TakeAttribute("name");
            if (name == null)
                throw new CompilerException("Section element must have a name attribute", Locate(node.Element));

            IList<Chunk> sectionChunks;
            if (!SectionChunks.TryGetValue(name.Value, out sectionChunks))
            {
                sectionChunks = new List<Chunk>();
                SectionChunks.Add(name.Value, sectionChunks);
            }

            var scope = new ScopeChunk { Position = Locate(inspector.OriginalNode) };
            sectionChunks.Add(scope);
            using (new Frame(this, scope.Body))
            {
                foreach (var attr in inspector.Attributes)
                {
                    Chunks.Add(new LocalVariableChunk { Name = attr.Name, Value = AsCode(attr), Position = Locate(attr) });
                }

                Accept(inspector.Body);
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
                using (new Frame(this, contentChunk.Body))
                    Accept(inspector.Body);
            }
            else if (varAttr != null || defAttr != null)
            {
                var variableChunk = new LocalVariableChunk { Name = AsCode(varAttr ?? defAttr), Type = "string" };
                Chunks.Add(variableChunk);

                var contentSetChunk = new ContentSetChunk { Variable = variableChunk.Name, Position = Locate(inspector.OriginalNode) };
                Chunks.Add(contentSetChunk);
                using (new Frame(this, contentSetChunk.Body))
                    Accept(inspector.Body);
            }
            else if (setAttr != null)
            {
                var addAttr = inspector.TakeAttribute("add");

                var contentSetChunk = new ContentSetChunk { Variable = AsCode(setAttr), Position = Locate(inspector.OriginalNode) };

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
                using (new Frame(this, contentSetChunk.Body))
                    Accept(inspector.Body);
            }
            else
            {
                throw new CompilerException("content element must have name, var, def, or set attribute");
            }
        }

        private void VisitCache(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            var keyAttr = inspector.TakeAttribute("key");
            var expiresAttr = inspector.TakeAttribute("expires");
            var signalAttr = inspector.TakeAttribute("signal");

            var chunk = new CacheChunk { Position = Locate(specialNode.Element) };

            if (keyAttr != null)
                chunk.Key = AsCode(keyAttr);
            else
                chunk.Key = "\"\"";

            if (expiresAttr != null)
                chunk.Expires = AsCode(expiresAttr);
            else
                chunk.Expires = "";

            if (signalAttr != null)
                chunk.Signal = AsCode(signalAttr);
            else
                chunk.Signal = "";

            Chunks.Add(chunk);
            using (new Frame(this, chunk.Body))
                Accept(inspector.Body);
        }


        private void VisitIf(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            var conditionAttr = inspector.TakeAttribute("condition") ?? inspector.TakeAttribute("if");

            var onceAttr = inspector.TakeAttribute("once");

            if (conditionAttr == null && onceAttr == null)
            {
                throw new CompilerException("Element must contain an if, condition, or once attribute");
            }

            Frame ifFrame = null;
            if (conditionAttr != null)
            {
                var ifChunk = new ConditionalChunk { Type = ConditionalType.If, Condition = AsCode(conditionAttr), Position = Locate(inspector.OriginalNode) };
                Chunks.Add(ifChunk);
                ifFrame = new Frame(this, ifChunk.Body);
            }

            Frame onceFrame = null;
            if (onceAttr != null)
            {
                var onceChunk = new ConditionalChunk { Type = ConditionalType.Once, Condition = onceAttr.AsCodeInverted(), Position = Locate(inspector.OriginalNode) };
                Chunks.Add(onceChunk);
                onceFrame = new Frame(this, onceChunk.Body);
            }

            Accept(specialNode.Body);

            if (onceFrame != null)
                onceFrame.Dispose();

            if (ifFrame != null)
                ifFrame.Dispose();
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
                using (new Frame(this, elseChunk.Body))
                    Accept(inspector.Body);
            }
            else
            {
                var elseIfChunk = new ConditionalChunk { Type = ConditionalType.ElseIf, Condition = AsCode(ifAttr), Position = Locate(inspector.OriginalNode) };
                Chunks.Add(elseIfChunk);
                using (new Frame(this, elseIfChunk.Body))
                    Accept(inspector.Body);
            }
        }

        private void VisitElseIf(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            if (!SatisfyElsePrecondition())
                throw new CompilerException("An 'elseif' may only follow an 'if' or 'elseif'.");

            var conditionAttr = inspector.TakeAttribute("condition");
            var elseIfChunk = new ConditionalChunk { Type = ConditionalType.ElseIf, Condition = AsCode(conditionAttr), Position = Locate(inspector.OriginalNode) };
            Chunks.Add(elseIfChunk);
            using (new Frame(this, elseIfChunk.Body))
                Accept(specialNode.Body);
        }

        private void VisitFor(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            var eachAttr = inspector.TakeAttribute("each");

            var forEachChunk = new ForEachChunk { Code = AsCode(eachAttr), Position = Locate(specialNode.Element) };
            Chunks.Add(forEachChunk);
            using (new Frame(this, forEachChunk.Body))
            {
                foreach (var attr in inspector.Attributes)
                {
                    Chunks.Add(new AssignVariableChunk { Name = attr.Name, Value = AsCode(attr), Position = Locate(attr) });
                }

                Accept(specialNode.Body);
            }
        }

        private void VisitSet(SpecialNodeInspector inspector)
        {
            foreach (var attr in inspector.Attributes)
            {
                Chunks.Add(new AssignVariableChunk { Name = attr.Name, Value = AsCode(attr), Position = Locate(attr) });
            }
        }

        private void VisitViewdata(SpecialNodeInspector inspector)
        {
            var defaultAttr = inspector.TakeAttribute("default");
            Snippets defaultValue = null;
            if (defaultAttr != null)
                defaultValue = AsCode(defaultAttr);

            var modelAttr = inspector.TakeAttribute("model");
            if (modelAttr != null)
            {
                var typeInspector = new TypeInspector(AsCode(modelAttr));
                AddUnordered(new ViewDataModelChunk { TModel = typeInspector.Type, TModelAlias = typeInspector.Name });
            }

            foreach (var attr in inspector.Attributes)
            {
                var typeInspector = new TypeInspector(AsCode(attr));
                AddUnordered(new ViewDataChunk
                                 {
                                     Type = typeInspector.Type,
                                     Name = typeInspector.Name ?? attr.Name,
                                     Key = attr.Name,
                                     Default = defaultValue,
                                     Position = Locate(attr)
                                 });
            }
        }

        private void VisitGlobal(SpecialNode specialNode)
        {
            var typeAttr = specialNode.Element.Attributes.FirstOrDefault(attr => attr.Name == "type");
            var type = typeAttr != null ? AsCode(typeAttr) : (Snippets)"object";

            foreach (var attr in specialNode.Element.Attributes.Where(a => a != typeAttr))
            {
                AddUnordered(new GlobalVariableChunk { Type = type, Name = attr.Name, Value = AsCode(attr) });
            }
        }

        private void VisitVar(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            Frame frame = null;
            if (!specialNode.Element.IsEmptyElement)
            {
                var scope = new ScopeChunk { Position = Locate(specialNode.Element) };
                Chunks.Add(scope);
                frame = new Frame(this, scope.Body);
            }

            var typeAttr = inspector.TakeAttribute("type");
            var type = typeAttr != null ? AsCode(typeAttr) : (Snippets)"var";

            foreach (var attr in inspector.Attributes)
            {
                Chunks.Add(new LocalVariableChunk { Type = type, Name = attr.Name, Value = AsCode(attr), Position = Locate(attr) });
            }

            Accept(specialNode.Body);

            if (frame != null)
                frame.Dispose();
        }

        private void VisitDefault(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            Frame frame = null;
            if (!specialNode.Element.IsEmptyElement)
            {
                var scope = new ScopeChunk { Position = Locate(specialNode.Element) };
                Chunks.Add(scope);
                frame = new Frame(this, scope.Body);
            }

            var typeAttr = inspector.TakeAttribute("type");
            var type = typeAttr != null ? AsCode(typeAttr) : (Snippets)"var";

            foreach (var attr in inspector.Attributes)
            {
                Chunks.Add(new DefaultVariableChunk { Type = type, Name = attr.Name, Value = AsCode(attr), Position = Locate(attr) });
            }

            Accept(specialNode.Body);

            if (frame != null)
                frame.Dispose();
        }

        private void VisitMarkdown(SpecialNode specialNode, SpecialNodeInspector inspector)
        {
            var markdownChunk = new MarkdownChunk();

            Chunks.Add(markdownChunk);
            using (new Frame(this, markdownChunk.Body))
            {
                Accept(inspector.Body);
            }
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
            var extensionChunk = new ExtensionChunk { Extension = extensionNode.Extension, Position = Locate(extensionNode) };
            Chunks.Add(extensionChunk);
            using (new Frame(this, extensionChunk.Body))
                extensionNode.Extension.VisitNode(this, extensionNode.Body, Chunks);
        }



    }
}
