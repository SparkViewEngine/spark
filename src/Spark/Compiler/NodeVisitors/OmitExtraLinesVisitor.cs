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

using System.Collections.Generic;
using System.Linq;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public class OmitExtraLinesVisitor : NodeVisitor<object>
    {
        public OmitExtraLinesVisitor(VisitorContext context) : base(context)
        {
        }

        protected override void Visit(SpecialNode node)
        {
            RemoveExtraLine();
            Nodes.Add(node);
            PushFrame(new List<Node>(), null);
            Accept(node.Body);
            RemoveExtraLine();
            node.Body = Nodes;
            PopFrame();
        }

        private void RemoveExtraLine()
        {
            var lastText = Nodes.LastOrDefault() as TextNode;
            if (lastText == null)
                return;

            var takeWhitespace = lastText.Text.TrimEnd(' ', '\t');
            if (takeWhitespace.EndsWith("\n"))
                lastText.Text = takeWhitespace.Substring(0, takeWhitespace.Length - 1).TrimEnd(' ', '\t', '\r');
        }
    }
}
