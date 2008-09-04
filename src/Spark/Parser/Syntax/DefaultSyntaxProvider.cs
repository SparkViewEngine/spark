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

        public override IList<Chunk> GetChunks(string viewPath, IViewFolder viewFolder, ISparkExtensionFactory extensionFactory)
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

            foreach(var visitor in BuildNodeVisitors(partialFileNames, extensionFactory))
            {
                visitor.Accept(nodes);
                nodes = visitor.Nodes;                
            }

            var chunkBuilder = new ChunkBuilderVisitor(result.Rest.GetPaint());
            chunkBuilder.Accept(nodes);
            return chunkBuilder.Chunks;
        }

        private IList<INodeVisitor> BuildNodeVisitors(IList<string> partialFileNames, ISparkExtensionFactory extensionFactory)
        {
            return new INodeVisitor[]
                       {
                           new PrefixExpandingVisitor(),
                           new SpecialNodeVisitor(partialFileNames, extensionFactory),
                           new ForEachAttributeVisitor(),
                           new ConditionalAttributeVisitor(),
                           new OmitExtraLinesVisitor(),
                           new TestElseElementVisitor(),
                           new UrlAttributeVisitor()
                       };
        }
    }
}
