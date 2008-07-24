using System;
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

            var nodes = _grammar.Nodes(position);
            if (nodes.Rest.PotentialLength() != 0)
            {
                ThrowParseException(viewPath, position, nodes.Rest);
            }

            var partialFileNames = FindPartialFiles(viewPath, viewFolder);

            var specialNodeVisitor = new SpecialNodeVisitor(partialFileNames, extensionFactory);
            specialNodeVisitor.Accept(nodes.Value);

            var forEachAttributeVisitor = new ForEachAttributeVisitor();
            forEachAttributeVisitor.Accept(specialNodeVisitor.Nodes);

            var conditionalAttributeVisitor = new ConditionalAttributeVisitor();
            conditionalAttributeVisitor.Accept(forEachAttributeVisitor.Nodes);

            var testElseElementVisitor = new TestElseElementVisitor();
            testElseElementVisitor.Accept(conditionalAttributeVisitor.Nodes);

            var chunkBuilder = new ChunkBuilderVisitor();
            chunkBuilder.Accept(testElseElementVisitor.Nodes);
            return chunkBuilder.Chunks;
        }
    }
}
