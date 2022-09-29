using System;
using System.Collections.Generic;
using Spark.Spool;

namespace Spark.Caching
{
    public class CacheMemento
    {
        public CacheMemento()
        {
            Content = new Dictionary<string, TextWriterMemento>();
            OnceTable = new Dictionary<string, string>();
        }

        public SpoolWriter SpoolOutput { get; set; }
        public Dictionary<string, TextWriterMemento> Content { get; set;}
        public Dictionary<string, string> OnceTable { get; set; }
    }
}
