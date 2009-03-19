// Copyright 2008 Louis DeJardin - http://whereslou.com
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class NamespaceVisitor : NodeVisitor<NamespaceVisitor.Frame>
    {
        public NamespaceVisitor(VisitorContext context)
            : base(context)
        {
            FrameData.Nametable = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(context.Prefix))
            {
                Context.Namespaces = NamespacesType.Qualified;
                FrameData.Nametable[context.Prefix] = Constants.Namespace;
                FrameData.Nametable["content"] = Constants.ContentNamespace;
                FrameData.Nametable["use"] = Constants.UseNamespace;
                FrameData.Nametable["macro"] = Constants.MacroNamespace;
                FrameData.Nametable["section"] = Constants.SectionNamespace;
                FrameData.Nametable["render"] = Constants.RenderNamespace;
            }
        }

        public class Frame
        {
            public IDictionary<string, string> Nametable { get; set; }
            public string ElementName { get; set; }
            public int ElementNameDepth { get; set; }
        }

        protected override void Visit(ElementNode node)
        {
            IDictionary<string, string> nametable = null;
            foreach (var xmlnsAttr in node.Attributes.Where(IsXmlnsAttribute).Where(IsKnownUri))
            {
                // create a nametable based on existing context
                nametable = nametable ?? new Dictionary<string, string>(FrameData.Nametable);

                if (xmlnsAttr.Name == "xmlns")
                    nametable[""] = xmlnsAttr.Value;
                else
                    nametable[xmlnsAttr.Name.Substring("xmlns:".Length)] = xmlnsAttr.Value;
            }

            if (nametable != null)
            {
                Context.Namespaces = NamespacesType.Qualified;

                // create a frame for this element
                PushFrame(Nodes, new Frame { Nametable = nametable, ElementName = node.Name });
            }
            else if (FrameData.ElementName == node.Name && !node.IsEmptyElement)
            {
                // protect the existing frame against the matching close
                FrameData.ElementNameDepth++;
            }

            ApplyNamespaces(node);

            if (nametable != null && node.IsEmptyElement)
                PopFrame();

            base.Visit(node);
        }

        protected override void Visit(EndElementNode node)
        {
            ApplyNamespaces(node);
            if (node.Name == FrameData.ElementName)
            {
                // remove frame once all expected close elements are passed
                if (FrameData.ElementNameDepth-- == 0)
                    PopFrame();
            }
            base.Visit(node);
        }

        private void ApplyNamespaces(ElementNode node)
        {
            var colonIndex = node.Name.IndexOf(':');
            if (colonIndex > 0)
            {
                var prefix = node.Name.Substring(0, colonIndex);
                string ns;
                if (FrameData.Nametable.TryGetValue(prefix, out ns))
                {
                    node.Namespace = ns;
                }
            }

            foreach (var attr in node.Attributes)
            {
                colonIndex = attr.Name.IndexOf(':');
                if (colonIndex <= 0) continue;

                var prefix = attr.Name.Substring(0, colonIndex);
                string ns;
                if (FrameData.Nametable.TryGetValue(prefix, out ns))
                {
                    attr.Namespace = ns;
                }
            }
        }

        private void ApplyNamespaces(EndElementNode node)
        {
            var colonIndex = node.Name.IndexOf(':');
            if (colonIndex > 0)
            {
                var prefix = node.Name.Substring(0, colonIndex);
                string ns;
                if (FrameData.Nametable.TryGetValue(prefix, out ns))
                {
                    node.Namespace = ns;
                }
            }
        }

        private static bool IsXmlnsAttribute(AttributeNode attr)
        {
            return attr.Name.StartsWith("xmlns:") || attr.Name == "xmlns";
        }

        static bool IsKnownUri(AttributeNode attr)
        {
            return attr.Value.StartsWith(Constants.Namespace) ||
                attr.Value == Constants.XIncludeNamespace;
        }
    }
}
