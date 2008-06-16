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

namespace Spark.Compiler.NodeVisitors
{
    public class ChunkBuilderVisitor : NodeVisitor
    {
        public IList<Chunk> Chunks { get; set; }

        public ChunkBuilderVisitor()
        {
            Chunks = new List<Chunk>();
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
            Chunks.Add(new SendExpressionChunk { Code = UnarmorCode(expressionNode.Code) });
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
            var prior = Chunks;
            try
            {
                var inspector = new SpecialNodeInspector(specialNode);
                switch (inspector.Name)
                {
                    case "var":
                        {
                            if (!specialNode.Element.IsEmptyElement)
                            {
                                var scope = new ScopeChunk();
                                Chunks.Add(scope);
                                Chunks = scope.Body;
                            }

                            var typeAttr = inspector.TakeAttribute("var");
                            string type = typeAttr != null ? typeAttr.Value : "var";

                            foreach (var attr in inspector.Attributes)
                            {
                                Chunks.Add(new LocalVariableChunk { Type = UnarmorCode(type), Name = attr.Name, Value = UnarmorCode(attr.Value) });
                            }

                            Accept(specialNode.Body);
                        }
                        break;
                    case "global":
                        {
                            var typeAttr = specialNode.Element.Attributes.FirstOrDefault(attr => attr.Name == "type");
                            string type = typeAttr != null ? typeAttr.Value : "object";

                            foreach (var attr in specialNode.Element.Attributes.Where(a => a != typeAttr))
                            {
                                AddUnordered(new GlobalVariableChunk { Type = UnarmorCode(type), Name = attr.Name, Value = UnarmorCode(attr.Value) });
                            }
                        }
                        break;
                    case "viewdata":
                        {
                            var modelAttr = inspector.TakeAttribute("model");
                            if (modelAttr != null)
                                AddUnordered(new ViewDataModelChunk { TModel = modelAttr.Value });

                            foreach (var attr in inspector.Attributes)
                            {
                                string typeName = UnarmorCode(attr.Value);
                                AddUnordered(new ViewDataChunk { Type = typeName, Name = attr.Name });
                            }
                        }
                        break;
                    case "set":
                        {
                            foreach (var attr in inspector.Attributes)
                            {
                                Chunks.Add(new AssignVariableChunk { Name = attr.Name, Value = attr.Value });
                            }
                        }
                        break;
                    case "for":
                        {
                            var eachAttr = specialNode.Element.Attributes.FirstOrDefault(attr => attr.Name == "each");

                            var forEachChunk = new ForEachChunk { Code = eachAttr.Value };
                            Chunks.Add(forEachChunk);
                            Chunks = forEachChunk.Body;

                            foreach (var attr in specialNode.Element.Attributes.Where(a => a != eachAttr))
                            {
                                Chunks.Add(new AssignVariableChunk { Name = attr.Name, Value = attr.Value });
                            }

                            Accept(specialNode.Body);
                        }
                        break;
                    case "if":
                        {
                            var conditionAttr = inspector.TakeAttribute("condition");

                            var ifChunk = new ConditionalChunk { Type = ConditionalType.If, Condition = UnarmorCode(conditionAttr.Value) };
                            Chunks.Add(ifChunk);
                            Chunks = ifChunk.Body;
                            Accept(specialNode.Body);
                        }
                        break;
                    case "else":
                        {
                            if (!SatisfyElsePrecondition())
                                throw new CompilerException("An 'else' may only follow an 'if' or 'elseif'.");

                            var elseChunk = new ConditionalChunk { Type = ConditionalType.Else };
                            Chunks.Add(elseChunk);
                            Chunks = elseChunk.Body;
                            Accept(specialNode.Body);
                        }
                        break;
                    case "elseif":
                        {
                            if (!SatisfyElsePrecondition())
                                throw new CompilerException("An 'elseif' may only follow an 'if' or 'elseif'.");

                            var conditionAttr = inspector.TakeAttribute("condition");
                            var elseIfChunk = new ConditionalChunk { Type = ConditionalType.ElseIf, Condition = UnarmorCode(conditionAttr.Value) };
                            Chunks.Add(elseIfChunk);
                            Chunks = elseIfChunk.Body;
                            Accept(specialNode.Body);
                        }
                        break;
                    case "content":
                        {
                            var nameAttr = specialNode.Element.Attributes.FirstOrDefault(attr => attr.Name == "name");

                            var contentChunk = new ContentChunk { Name = nameAttr.Value };
                            Chunks.Add(contentChunk);
                            Chunks = contentChunk.Body;
                            Accept(specialNode.Body);
                        }
                        break;
                    case "use":
                        {
                            //TODO: change <use file=""> to <render partial="">, to avoid
                            // random attribute conflicts on parameterized cases

                            var content = inspector.TakeAttribute("content");
                            var file = inspector.TakeAttribute("file");
                            var namespaceAttr = inspector.TakeAttribute("namespace");
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
                                    Chunks.Add(new LocalVariableChunk { Name = attr.Name, Value = attr.Value });
                                }

                                var useFileChunk = new RenderPartialChunk { Name = file.Value };
                                Chunks.Add(useFileChunk);
                            }
                            else if (namespaceAttr != null)
                            {
                                var useNamespaceChunk = new UseNamespaceChunk { Namespace = namespaceAttr.Value };
                                AddUnordered(useNamespaceChunk);
                            }
                            else
                            {
                                throw new CompilerException("Special node use had no understandable attributes");
                            }
                        }
                        break;
                    case "macro":
                        {
                            var name = inspector.TakeAttribute("name");
                            var macro = new MacroChunk { Name = name.Value };
                            foreach (var attr in inspector.Attributes)
                            {
                                macro.Parameters.Add(new MacroParameter {Name = attr.Name, Type = UnarmorCode(attr.Value)});
                            }
                            Chunks.Add(macro);
                            Chunks = macro.Body;
                            Accept(specialNode.Body);
                        }
                        break;
                    default:
                        throw new CompilerException(string.Format("Unknown special node {0}", specialNode.Element.Name));
                }
            }
            finally
            {
                Chunks = prior;
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
