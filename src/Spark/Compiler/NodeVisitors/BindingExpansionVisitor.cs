using System;
using System.Collections.Generic;
using System.Linq;
using Spark.Bindings;
using Spark.Parser.Code;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class BindingExpansionVisitor : NodeVisitor
    {
        public BindingExpansionVisitor(VisitorContext context)
            : base(context)
        {
        }

        protected override void Visit(ElementNode element)
        {
            var binding = MatchElementBinding(element);
            if (binding == null)
            {
                base.Visit(element);
                return;
            }

            var snippets = binding.Nodes.SelectMany(bindingNode => BuildSnippetsForNode(binding, bindingNode, element));
            var expression = new ExpressionNode(snippets);
            Accept(expression);
        }



        private Binding MatchElementBinding(ElementNode node)
        {
            var bindingsForName = Context.Bindings.Where(binding => binding.ElementName == node.Name);
            var withAttributesSatisfied = bindingsForName.Where(binding => RequiredAttributesSatisfied(binding, node));
            return withAttributesSatisfied.FirstOrDefault();
        }

        private static bool RequiredAttributesSatisfied(Binding binding, ElementNode element)
        {
            // any xpath targetting a flat name must be present, or the binding doesn't qualify
            foreach (var reference in binding.Nodes.OfType<BindingNameReference>())
            {
                var nameReference = reference;
                if (!element.Attributes.Any(attr => attr.Name == nameReference.Name))
                    return false;
            }
            return true;
        }


        private static IEnumerable<Snippet> BuildSnippetsForNode(Binding binding, BindingNode node, ElementNode element)
        {
            if (node is BindingLiteral)
                return BuildSnippets(node as BindingLiteral);
            if (node is BindingNameReference)
                return BuildSnippets(node as BindingNameReference, element);
            if (node is BindingPrefixReference)
                return BuildSnippets(binding, node as BindingPrefixReference, element);

            throw new CompilerException("Binding node type " + node.GetType() + " not understood");
        }

        private static IEnumerable<Snippet> BuildSnippets(BindingLiteral literal)
        {
            return new[] { new Snippet { Value = literal.Text } };
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
                .Where(attr => binding.Nodes.Any(compare => TestBetterMatch(attr.Name, prefix.Prefix, compare)) == false);

            // remaining attributes have a name that doesn't include the prefix characters
            var attrs = remaining
                .Select(attr => new { PropertyName = attr.Name.Substring((prefix.Prefix ?? "").Length), Attribute = attr });

            var results = new List<Snippet>();

            var first = true;
            foreach (var attr in attrs)
            {
                if (first)
                    first = false;
                else
                    results.Add(new Snippet { Value = "," });

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
            }

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