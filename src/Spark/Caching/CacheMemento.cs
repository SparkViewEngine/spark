using System.Collections.Generic;
using Spark.Spool;

namespace Spark.Caching
{
    public class CacheMemento
    {
        public SpoolWriter SpoolOutput { get; set; }

        public Dictionary<string, TextWriterMemento> Content { get; set; } = new();

        public Dictionary<string, string> OnceTable { get; set; } = new();
    }
}
