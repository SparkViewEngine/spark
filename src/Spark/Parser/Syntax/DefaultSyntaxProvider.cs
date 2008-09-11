using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.FileSystem;
using Spark.Parser.Markup;

namespace Spark.Parser.Syntax
{
    public class DefaultSyntaxProvider : AbstractSyntaxProvider
    {
        static readonly MarkupGrammar _grammar = new MarkupGrammar();

        public override IList<Chunk> GetChunks(VisitorContext context, string path)
        {
            context.SyntaxProvider = this;
            context.ViewPath = path;
            context.PartialFileNames = FindPartialFiles(path, context.ViewFolder);

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

            if (parse == "text")
            {
                var encoded = sourceContext.Content
                    .Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");
                return new[] { new TextNode(encoded) };
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

        private IList<INodeVisitor> BuildNodeVisitors(VisitorContext context)
        {
            return new INodeVisitor[]
                       {
                           new NamespaceVisitor(context),
                           new IncludeVisitor(context),
                           new PrefixExpandingVisitor(context),
                           new SpecialNodeVisitor(context),
                           new ForEachAttributeVisitor(context),
                           new ConditionalAttributeVisitor(context),
                           new OmitExtraLinesVisitor(context),
                           new TestElseElementVisitor(context),
                           new UrlAttributeVisitor(context)
                       };
        }
    }
}
