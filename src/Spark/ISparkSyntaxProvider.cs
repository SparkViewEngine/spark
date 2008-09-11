using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Markup;

namespace Spark
{
    public interface ISparkSyntaxProvider
    {
        IList<Chunk> GetChunks(VisitorContext context, string path);
        IList<Node> IncludeFile(VisitorContext context, string path, string parse);
    }
}
