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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class IncludeVisitor : NodeVisitor<IncludeVisitor.Frame>
    {
        public IncludeVisitor(VisitorContext context)
            : base(context)
        {
        }

        public enum Mode
        {
            NormalContent,
            SuccessfulInclude,
            FailedInclude,
            IgnoringFallback
        }

        public class Frame
        {
            public Mode Mode { get; set; }
            public Exception IncludeException { get; set; }
            public IList<Node> NodesForFallback { get; set; }
            public bool FallbackUsed;

            public int RedundantDepth { get; set; }
        }

        protected override void Visit(ElementNode node)
        {
            if (NameUtility.IsMatch("include", Constants.XIncludeNamespace, node.Name, node.Namespace, Context.Namespaces))
            {
                if (FrameData.Mode == Mode.SuccessfulInclude ||
                    FrameData.Mode == Mode.FailedInclude)
                {
                    if (!node.IsEmptyElement)
                        FrameData.RedundantDepth++;
                    return;
                }

                var href = node.Attributes.FirstOrDefault(a => a.Name == "href");
                var parse = node.Attributes.FirstOrDefault(a => a.Name == "parse");
                BeginInclude(href.Value, parse == null ? "xml" : parse.Value);
                if (node.IsEmptyElement)
                    EndInclude();
                return;
            }

            if (NameUtility.IsMatch("fallback", Constants.XIncludeNamespace, node.Name, node.Namespace, Context.Namespaces))
            {
                if (FrameData.Mode == Mode.IgnoringFallback)
                {
                    if (!node.IsEmptyElement)
                        FrameData.RedundantDepth++;
                    return;
                }

                BeginFallback();
                if (node.IsEmptyElement)
                    EndFallback();
                return;
            }

            base.Visit(node);
        }

        protected override void Visit(EndElementNode node)
        {
            if (NameUtility.IsMatch("include", Constants.XIncludeNamespace, node.Name, node.Namespace, Context.Namespaces))
            {
                if (FrameData.Mode != Mode.FailedInclude &&
                    FrameData.Mode != Mode.SuccessfulInclude)
                {
                    throw new CompilerException("Unexpected </include> element");
                }
                if (FrameData.RedundantDepth-- == 0)
                    EndInclude();
                return;
            }
            if (NameUtility.IsMatch("fallback", Constants.XIncludeNamespace, node.Name, node.Namespace, Context.Namespaces))
            {
                if (FrameData.Mode != Mode.NormalContent &&
                    FrameData.Mode != Mode.IgnoringFallback)
                {
                    throw new CompilerException("Unexpected </fallback> element");
                }
                if (FrameData.RedundantDepth-- == 0)
                    EndFallback();
                return;
            }
            base.Visit(node);
        }

        void BeginInclude(string href, string parse)
        {
            try
            {
                var nodes = Context.SyntaxProvider.IncludeFile(Context, href, parse);
                foreach (var addNode in nodes)
                    Nodes.Add(addNode);

                PushFrame(new List<Node>(), new Frame { Mode = Mode.SuccessfulInclude });
            }
            catch (FileNotFoundException ex)
            {

                PushFrame(new List<Node>(), new Frame { Mode = Mode.FailedInclude, IncludeException = ex, NodesForFallback = Nodes });
            }
        }

        void EndInclude()
        {
            var frame = FrameData;
            PopFrame();

            if (frame.Mode == Mode.FailedInclude && frame.FallbackUsed == false)
            {
                throw new CompilerException(frame.IncludeException.Message);
            }

        }

        void BeginFallback()
        {
            if (FrameData.Mode == Mode.SuccessfulInclude)
            {
                PushFrame(Nodes, new Frame {Mode = Mode.IgnoringFallback});
                return;
            }
            if (FrameData.Mode != Mode.FailedInclude)
            {
                throw new CompilerException("<fallback> only valid inside <include>");
            }
            FrameData.FallbackUsed = true;
            PushFrame(FrameData.NodesForFallback, new Frame {Mode = Mode.NormalContent});
        }

        void EndFallback()
        {
            PopFrame();
        }
    }
}
