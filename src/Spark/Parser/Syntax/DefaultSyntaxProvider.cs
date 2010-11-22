//-------------------------------------------------------------------------
// <copyright file="Constraints.cs">
// Copyright 2008-2010 Louis DeJardin - http://whereslou.com
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
// </copyright>
// <author>Louis DeJardin</author>
// <author>John Gietzen</author>
//-------------------------------------------------------------------------

namespace Spark.Parser.Syntax
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Spark.Compiler;
    using Spark.Compiler.NodeVisitors;
    using Spark.Parser.Code;
    using Spark.Parser.Markup;

    public class DefaultSyntaxProvider : AbstractSyntaxProvider
    {
        private readonly MarkupGrammar _grammar;

        public DefaultSyntaxProvider(IParserSettings settings)
        {
            _grammar = new MarkupGrammar(settings);
        }

        public override IList<Chunk> GetChunks(VisitorContext context, string path)
        {
            context.SyntaxProvider = this;
            context.ViewPath = path;

            var sourceContext = CreateSourceContext(context.ViewPath, context.ViewFolder);
            var position = new Position(sourceContext);

            var result = _grammar.Nodes(position);
            if (result.Rest.PotentialLength() != 0)
            {
                ThrowParseException(context.ViewPath, position, result.Rest);
            }

            context.Paint = result.Rest.GetPaint();

            var nodes = result.Value;
            foreach (var visitor in BuildNodeVisitors(context))
            {
                visitor.Accept(nodes);
                nodes = visitor.Nodes;
            }

            var chunkBuilder = new ChunkBuilderVisitor(context);
            chunkBuilder.Accept(nodes);
            return chunkBuilder.Chunks;
        }

        public override IList<Node> IncludeFile(VisitorContext context, string path, string parse)
        {
            var existingPath = context.ViewPath;

            var directoryPath = Path.GetDirectoryName(context.ViewPath);
            var relativePath = path.Replace('/', '\\');
            while (relativePath.StartsWith("..\\"))
            {
                directoryPath = Path.GetDirectoryName(directoryPath);
                relativePath = relativePath.Substring(3);
            }
            context.ViewPath = Path.Combine(directoryPath, relativePath);

            var sourceContext = CreateSourceContext(context.ViewPath, context.ViewFolder);

            switch (parse)
            {
                case "text":
                    var encoded = sourceContext.Content
                        .Replace("&", "&amp;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;");
                    return new[] {new TextNode(encoded)};
                case "html":
                    return new[] {new TextNode(sourceContext.Content)};
            }


            var position = new Position(sourceContext);
            var result = _grammar.Nodes(position);
            if (result.Rest.PotentialLength() != 0)
            {
                ThrowParseException(context.ViewPath, position, result.Rest);
            }
            context.Paint = context.Paint.Union(result.Rest.GetPaint());

            var namespaceVisitor = new NamespaceVisitor(context);
            namespaceVisitor.Accept(result.Value);

            var includeVisitor = new IncludeVisitor(context);
            includeVisitor.Accept(namespaceVisitor.Nodes);

            context.ViewPath = existingPath;
            return includeVisitor.Nodes;
        }

        public override Snippets ParseFragment(Position begin, Position end)
        {
            var result = _grammar.Expression(begin.Constrain(end));

            var unparsedLength = result.Rest.PotentialLength();
            if (unparsedLength == 0)
                return result.Value;

            var snippets = new Snippets(result.Value);

            snippets.Add(new Snippet
            {
                Value = result.Rest.Peek(unparsedLength),
                Begin = result.Rest,
                End = result.Rest.Advance(unparsedLength)
            });

            return snippets;

        }

        private IList<INodeVisitor> BuildNodeVisitors(VisitorContext context)
        {
            return new INodeVisitor[]
                       {
                           new NamespaceVisitor(context),
                           new IncludeVisitor(context),
                           new PrefixExpandingVisitor(context),
                           new SpecialNodeVisitor(context),
                           new CacheAttributeVisitor(context),
                           new WhitespaceCleanerVisitor(context),
                           new ForEachAttributeVisitor(context),
                           new ConditionalAttributeVisitor(context),
                           new TestElseElementVisitor(context),
                           new OnceAttributeVisitor(context),
                           new UrlAttributeVisitor(context),
                           new BindingExpansionVisitor(context)
                       };
        }
    }
}
