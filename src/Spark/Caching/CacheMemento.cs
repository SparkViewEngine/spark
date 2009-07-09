using System;
using System.Collections.Generic;
using Spark.Spool;

namespace Spark.Caching
{
    public class CacheMemento
    {
        public CacheMemento(SpoolWriter spoolOutput)
        {
            SpoolOutput = spoolOutput;
            Content = new Dictionary<string, TextWriterMemento>();
        }

        public SpoolWriter SpoolOutput { get; set; }
        public Dictionary<string, TextWriterMemento> Content { get; set;}
    }
}