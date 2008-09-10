using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class PrefixExpandingVisitor : NodeVisitor<PrefixExpandingVisitor.Frame>
    {
        readonly IList<PrefixSpecs> _prefixes = new List<PrefixSpecs>();

        public PrefixExpandingVisitor(VisitorContext context)
            : base(context)
        {
            _prefixes = new[]
                            {
                                new PrefixSpecs("section", Constants.SectionNamespace, "section", "name"),
                                new PrefixSpecs("macro", Constants.MacroNamespace, "macro", "name"),
                                new PrefixSpecs("content", Constants.ContentNamespace, "content", "name"),
                                new PrefixSpecs("use", Constants.UseNamespace, "use", "content"),
                                new PrefixSpecs("render", Constants.RenderNamespace, "render", "section")
                            };
        }


        public class PrefixSpecs
        {
            public PrefixSpecs(string prefix, string ns, string elementName, string attributeName)
            {
                Prefix = prefix;
                Namespace = ns;
                ElementName = elementName;
                AttributeName = attributeName;
            }

            public string Prefix { get; set; }
            public string Namespace { get; set; }
            public string ElementName { get; set; }
            public string AttributeName { get; set; }
        }

        public class Frame
        {
            public string OriginalElementName { get; set; }
            public PrefixSpecs Specs { get; set; }
        }

        protected override void Visit(ElementNode node)
        {
            var prefix = NameUtility.GetPrefix(node.Name);
            if (!string.IsNullOrEmpty(prefix))
            {
                var specs = _prefixes.FirstOrDefault(spec => IsMatchingSpec(spec, node));
                if (specs != null)
                {
                    PushReconstructedNode(node, specs);
                    return;
                }
            }

            base.Visit(node);
        }

        bool IsMatchingSpec(PrefixSpecs specs, ElementNode node)
        {
            if (Context.Namespaces == NamespacesType.Unqualified)
            {
                return specs.Prefix == NameUtility.GetPrefix(node.Name);
            }

            return specs.Namespace == node.Namespace;
        }

        private void PushReconstructedNode(ElementNode original, PrefixSpecs specs)
        {
            // For element <foo:blah> add an additional attributes like name="blah"
            var attributes = new List<AttributeNode>
                                 {
                                     new AttributeNode(specs.AttributeName, NameUtility.GetName(original.Name))
                                 };
            attributes.AddRange(original.Attributes);

            // Replace <foo:blah> with <foo>
            var reconstructed = new ElementNode(specs.ElementName, attributes, original.IsEmptyElement) { OriginalNode = original, Namespace = Constants.Namespace };
            Nodes.Add(reconstructed);

            // If it's not empty, add a frame to watch for the matching end element
            if (!original.IsEmptyElement)
            {
                PushFrame(Nodes, new Frame { OriginalElementName = original.Name, Specs = specs });
            }
        }

        protected override void Visit(EndElementNode node)
        {
            if (string.Equals(node.Name, FrameData.OriginalElementName))
            {
                // replace matching </foo:blah> with </foo>
                Nodes.Add(new EndElementNode(FrameData.Specs.ElementName) { Namespace = Constants.Namespace });
                PopFrame();
            }
            else
            {
                base.Visit(node);
            }
        }
    }
}
