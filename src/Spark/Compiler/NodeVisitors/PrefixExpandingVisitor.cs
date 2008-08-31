using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class PrefixExpandingVisitor : NodeVisitor<PrefixExpandingVisitor.Frame>
    {
        IList<PrefixSpecs> _prefixes = new List<PrefixSpecs>();

        public PrefixExpandingVisitor()
        {
            _prefixes = new[]
                            {
                                new PrefixSpecs("section", "section", "name"),
                                new PrefixSpecs("macro", "macro", "name"),
                                new PrefixSpecs("content", "content", "name"),
                                new PrefixSpecs("use", "use", "content")
                            };
        }


        public class PrefixSpecs
        {
            public PrefixSpecs(string prefix, string elementName, string attributeName)
            {
                Prefix = prefix;
                ElementName = elementName;
                AttributeName = attributeName;
            }
            public string Prefix { get; set; }
            public string ElementName { get; set; }
            public string AttributeName { get; set; }
        }

        public class Frame
        {
            public string OriginalElementName { get; set; }
            public PrefixSpecs Specs { get; set; }
        }

        static string GetPrefix(string elementName)
        {
            var colonIndex = elementName.IndexOf(':');
            return colonIndex <= 0 ? "" : elementName.Substring(0, colonIndex);
        }

        static string GetName(string elementName)
        {
            var colonIndex = elementName.IndexOf(':');
            return colonIndex <= 0 ? elementName : elementName.Substring(colonIndex + 1);
        }

        protected override void Visit(Spark.Parser.Markup.ElementNode node)
        {
            var prefix = GetPrefix(node.Name);
            if (!string.IsNullOrEmpty(prefix))
            {
                var specs = _prefixes.FirstOrDefault(spec => string.Equals(spec.Prefix, prefix));
                if (specs != null)
                {
                    PushReconstructedNode(node, specs);
                    return;
                }
            }

            base.Visit(node);
        }

        private void PushReconstructedNode(ElementNode original, PrefixSpecs specs)
        {
            // For element <foo:blah> add an additional attributes like name="blah"
            var attributes = new List<AttributeNode>();
            attributes.Add(new AttributeNode(specs.AttributeName, GetName(original.Name)));
            attributes.AddRange(original.Attributes);
            
            // Replace <foo:blah> with <foo>
            var reconstructed = new ElementNode(specs.ElementName, attributes, original.IsEmptyElement);
            Nodes.Add(reconstructed);

            // If it's not empty, add a frame to watch for the matching end element
            if (!original.IsEmptyElement)
            {
                PushFrame(Nodes, new Frame {OriginalElementName = original.Name, Specs = specs});
            }
        }

        protected override void Visit(Spark.Parser.Markup.EndElementNode node)
        {
            if (string.Equals(node.Name, FrameData.OriginalElementName))
            {
                // replace matching </foo:blah> with </foo>
                Nodes.Add(new EndElementNode(FrameData.Specs.ElementName));
                PopFrame();
            }
            else
            {
                base.Visit(node);
            }
        }
    }
}
