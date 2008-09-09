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

        public override IList<Chunk> GetChunks(string viewPath, IViewFolder viewFolder, ISparkExtensionFactory extensionFactory, string prefix)
        {
            var sourceContext = CreateSourceContext(viewPath, viewFolder);
            var position = new Position(sourceContext);

            var result = _grammar.Nodes(position);
            if (result.Rest.PotentialLength() != 0)
            {
                ThrowParseException(viewPath, position, result.Rest);
            }
            var nodes = result.Value;

            var partialFileNames = FindPartialFiles(viewPath, viewFolder);

            var context = new VisitorContext
                              {
                                  ExtensionFactory = extensionFactory,
                                  PartialFileNames = partialFileNames,
                                  Paint = result.Rest.GetPaint(),
                                  Prefix = prefix
                              };

            foreach(var visitor in BuildNodeVisitors(context))
            {
                visitor.Accept(nodes);
                nodes = visitor.Nodes;                
            }

            var chunkBuilder = new ChunkBuilderVisitor(context);
            chunkBuilder.Accept(nodes);
            return chunkBuilder.Chunks;
        }

        private IList<INodeVisitor> BuildNodeVisitors(VisitorContext context)
        {
            return new INodeVisitor[]
                       {
                           new NamespaceVisitor(context),
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
