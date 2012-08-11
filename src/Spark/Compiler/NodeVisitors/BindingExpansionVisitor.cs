using System;
using System.Collections.Generic;
using System.Linq;
using Spark.Bindings;
using Spark.Parser.Code;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class BindingExpansionVisitor : NodeVisitor<BindingExpansionVisitor.Frame>
    {
        public BindingExpansionVisitor(VisitorContext context)
            : base(context)
        {
        }

        public class Frame
        {
            public ElementNode Element { get; set; }
            public Binding Binding { get; set; }
            public int RedundantDepth { get; set; }
            public int NestingLevel { get; set; }
        }

        protected override void Visit(ElementNode element)
        {
            var binding = MatchElementBinding(element);
            if (binding == null)
            {
                if (!element.IsEmptyElement &&
                    FrameData.Binding != null &&
                    FrameData.Binding.ElementName == element.Name)
                {
                    ++FrameData.RedundantDepth;
                }

                base.Visit(element);
                return;
            }

            BeginBinding(element, binding);
            if (element.IsEmptyElement)
                EndBinding();
        }


        protected override void Visit(EndElementNode endElement)
        {
            if (FrameData.Binding != null && FrameData.Binding.ElementName == endElement.Name)
            {
                if (FrameData.RedundantDepth-- == 0)
                {
                    EndBinding();
                    return;
                }
            }
            base.Visit(endElement);
        }



        private void BeginBinding(ElementNode element, Binding binding)
        {
            if (binding.HasChildReference)
            {
                var stmt =
                    string.Format("{{var __bindingWriter{0} = new System.IO.StringWriter(); using(OutputScope(__bindingWriter{0})) {{",
                                  FrameData.NestingLevel);
                Accept(new StatementNode(stmt));
            }
            else
            {
                var phrase = binding.Phrases.First();
                ProcessPhrase(binding, phrase, element);
            }
            PushFrame(Nodes, new Frame { Binding = binding, Element = element, NestingLevel = FrameData.NestingLevel + 1 });
        }

        private void EndBinding()
        {
            var element = FrameData.Element;
            var binding = FrameData.Binding;
            PopFrame();

            if (binding.HasChildReference || binding.Phrases.Count() == 2)
            {
                if (binding.HasChildReference)
                    Accept(new StatementNode("}"));

                ProcessPhrase(binding, binding.Phrases.Last(), element);

                if (binding.HasChildReference)
                    Accept(new StatementNode("}"));
            }
        }

        private void ProcessPhrase(Binding binding, BindingPhrase phrase, ElementNode element)
        {
            var snippets = phrase.Nodes.SelectMany(bindingNode => BuildSnippetsForNode(binding, bindingNode, element));

            if (phrase.Type == BindingPhrase.PhraseType.Expression)
            {
                Accept(new ExpressionNode(snippets));
            }
            else if (phrase.Type == BindingPhrase.PhraseType.Statement)
            {
                Accept(new StatementNode(snippets));
            }
            else
            {
                throw new CompilerException("Unknown binding phrase type " + phrase.Type);
            }
        }

        private static IEnumerable<BindingNode> AllNodes(Binding binding)
        {
            return binding.Phrases.SelectMany(p => p.Nodes);
        }


        private Binding MatchElementBinding(ElementNode node)
        {
            if (Context.Bindings == null) return null;
            var bindingsForName = Context.Bindings.Where(binding => binding.ElementName == node.Name);
            var withAttributesSatisfied = bindingsForName.Where(binding => RequiredReferencesSatisfied(binding, node));
            return withAttributesSatisfied.FirstOrDefault();
        }

        private static bool RequiredReferencesSatisfied(Binding binding, ElementNode element)
        {
            // any xpath targetting a flat name must be present, or the binding doesn't qualify
            foreach (var reference in AllNodes(binding).OfType<BindingNameReference>())
            {
                var nameReference = reference;
                if (nameReference.Optional)
                    continue;
                if (!element.Attributes.Any(attr => attr.Name == nameReference.Name))
                    return false;
            }

            // a binding with child::* mapping won't match self-closing elements
            if (binding.HasChildReference && element.IsEmptyElement)
                return false;

            return true;
        }


        private IEnumerable<Snippet> BuildSnippetsForNode(Binding binding, BindingNode node, ElementNode element)
        {
            if (node is BindingLiteral)
                return BuildSnippets(node as BindingLiteral);
            if (node is BindingNameReference)
                return BuildSnippets(node as BindingNameReference, element);
            if (node is BindingPrefixReference)
                return BuildSnippets(binding, node as BindingPrefixReference, element);
            if (node is BindingChildReference)
                return BuildSnippets(node as BindingChildReference);

            throw new CompilerException("Binding node type " + node.GetType() + " not understood");
        }

        private static IEnumerable<Snippet> BuildSnippets(BindingLiteral literal)
        {
            return new[] { new Snippet { Value = literal.Text } };
        }

        private IEnumerable<Snippet> BuildSnippets(BindingChildReference literal)
        {
            return new[] { new Snippet { Value = "__bindingWriter" + FrameData.NestingLevel + ".ToString()" } };
        }

        private static IEnumerable<Snippet> BuildSnippets(BindingNameReference reference, ElementNode element)
        {
            var attrs = element.Attributes.Where(attr => attr.Name == reference.Name);

            if (reference.AssumeStringValue)
            {
                var builder = new ExpressionBuilder();
                PopulateBuilder(attrs.SelectMany(attr => attr.Nodes), builder);
                return new[] { new Snippet { Value = builder.ToCode() } };
            }

            return attrs.SelectMany(attr => attr.AsCode());
        }

        private static IEnumerable<Snippet> BuildSnippets(Binding binding, BindingPrefixReference prefix, ElementNode element)
        {
            // this reference can use all attributes that start with it's prefix
            var candidates = element.Attributes
                .Where(attr => attr.Name.StartsWith(prefix.Prefix ?? ""));

            // attributes that are matched by name, or by a longer prefix, no longer remain
            var remaining = candidates
                .Where(attr => AllNodes(binding).Any(compare => TestBetterMatch(attr.Name, prefix.Prefix, compare)) == false);

            // remaining attributes have a name that doesn't include the prefix characters
            var attrs = remaining
                .Select(attr => new { PropertyName = attr.Name.Substring((prefix.Prefix ?? "").Length), Attribute = attr });

            var results = new List<Snippet>();

            if (prefix.AssumeDictionarySyntax)
                results.Add(new Snippet { Value = "{" });

            var first = true;
            foreach (var attr in attrs)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    results.Add(new Snippet { Value = "," });
                }

                if (prefix.AssumeDictionarySyntax)
                    results.Add(new Snippet { Value = "{\"" + attr.PropertyName + "\"," });
                else
                    results.Add(new Snippet { Value = attr.PropertyName + "=" });

                if (prefix.AssumeStringValue)
                {
                    var builder = new ExpressionBuilder();
                    PopulateBuilder(attr.Attribute.Nodes, builder);
                    results.Add(new Snippet { Value = builder.ToCode() });
                }
                else
                {
                    results.AddRange(attr.Attribute.AsCode());
                }

                if (prefix.AssumeDictionarySyntax)
                    results.Add(new Snippet { Value = "}" });
            }

            if (prefix.AssumeDictionarySyntax)
                results.Add(new Snippet { Value = "}" });

            return results;
        }

        private static bool TestBetterMatch(string attributeName, string matchingPrefix, BindingNode compareNode)
        {
            if (compareNode is BindingNameReference)
            {
                var nameReference = (BindingNameReference)compareNode;
                if (attributeName == nameReference.Name)
                {
                    // an exact name reference will prevent any wildcard consumption
                    return true;
                }
            }

            if (compareNode is BindingPrefixReference)
            {
                var currentPrefix = matchingPrefix ?? "";
                var comparePrefix = ((BindingPrefixReference)compareNode).Prefix ?? "";

                if (comparePrefix.Length > currentPrefix.Length &&
                    attributeName.StartsWith(comparePrefix))
                {
                    // A longer wildcard reference which matches the current name will 
                    // prevent the shorter from using it
                    return true;
                }
            }

            // otherwise this match is good to go.
            return false;
        }

        private static void PopulateBuilder(IEnumerable<Node> nodes, ExpressionBuilder builder)
        {
            foreach (var node in nodes)
            {
                if (node is TextNode)
                {
                    var text = (TextNode)node;
                    builder.AppendLiteral(text.Text);
                }
                else if (node is EntityNode)
                {
                    var entity = (EntityNode)node;
                    builder.AppendLiteral("&" + entity.Name + ";");
                }
                else if (node is ExpressionNode)
                {
                    var expr = (ExpressionNode)node;
                    builder.AppendExpression(expr.Code);
                }
                else
                {
                    throw new CompilerException("Unknown content in attribute");
                }
            }
        }
    }
}