using System.Linq;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public abstract class SpecialAttributeVisitorBase : NodeVisitor<SpecialAttributeVisitorBase.Frame>
    {
        protected SpecialAttributeVisitorBase(VisitorContext context)
            : base(context)
        {
        }

        public class Frame
        {
            public string ClosingName { get; set; }
            public int ClosingNameOutstanding { get; set; }
        }

        protected abstract bool IsSpecialAttribute(ElementNode element, AttributeNode attribute);
        protected abstract SpecialNode CreateWrappingNode(AttributeNode eachAttr);

        protected override void Visit(ElementNode node)
        {
            var specialAttr = node.Attributes.FirstOrDefault(attr => IsSpecialAttribute(node, attr));
            if (specialAttr != null)
            {
                var wrapping = CreateWrappingNode(specialAttr);
                node.Attributes.Remove(specialAttr);
                wrapping.Body.Add(node);

                Nodes.Add(wrapping);
                if (!node.IsEmptyElement)
                {
                    PushFrame(wrapping.Body, new Frame { ClosingName = node.Name, ClosingNameOutstanding = 1 });
                }
            }
            else if (string.Equals(node.Name, FrameData.ClosingName) && !node.IsEmptyElement)
            {
                ++FrameData.ClosingNameOutstanding;
                Nodes.Add(node);
            }
            else
            {
                Nodes.Add(node);
            }
        }

        protected override void Visit(EndElementNode node)
        {
            Nodes.Add(node);

            if (string.Equals(node.Name, FrameData.ClosingName))
            {
                --FrameData.ClosingNameOutstanding;
                if (FrameData.ClosingNameOutstanding == 0)
                {
                    PopFrame();
                }
            }
        }

        protected override void Visit(SpecialNode node)
        {
            var reconstructed = new SpecialNode(node.Element);

            var nqName = NameUtility.GetName(node.Element.Name);

            AttributeNode specialAttr = null;
            if (nqName != "for")
                specialAttr = reconstructed.Element.Attributes.FirstOrDefault(attr => IsSpecialAttribute(node.Element, attr));

            if (specialAttr != null)
            {
                reconstructed.Element.Attributes.Remove(specialAttr);

                var wrapping = CreateWrappingNode(specialAttr);
                Nodes.Add(wrapping);
                PushFrame(wrapping.Body, new Frame());
            }

            Nodes.Add(reconstructed);
            PushFrame(reconstructed.Body, new Frame());
            Accept(node.Body);
            PopFrame();

            if (specialAttr != null)
            {
                PopFrame();
            }
        }

        protected override void Visit(ExtensionNode node)
        {
            var reconstructed = new ExtensionNode(node.Element, node.Extension);

            var eachAttr = reconstructed.Element.Attributes.FirstOrDefault(attr => IsSpecialAttribute(node.Element, attr));
            if (eachAttr != null)
            {
                reconstructed.Element.Attributes.Remove(eachAttr);

                var wrapping = CreateWrappingNode(eachAttr);
                Nodes.Add(wrapping);
                PushFrame(wrapping.Body, new Frame());
            }

            Nodes.Add(reconstructed);
            PushFrame(reconstructed.Body, new Frame());
            Accept(node.Body);
            PopFrame();

            if (eachAttr != null)
            {
                PopFrame();
            }
        }
    }
}