using Spark.Parser.Code;

namespace Spark.Compiler
{
    public class SourceMapping
    {
        public Snippet Source { get; set; }
        public int OutputBegin { get; set; }
        public int OutputEnd { get; set; }
    }
}