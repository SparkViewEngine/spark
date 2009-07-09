using System.Collections.Generic;
using System.IO;

namespace Spark.Caching
{
    public interface ICacheSubject
    {
        TextWriter Output { get; set; }
        Dictionary<string, TextWriter> Content { get; }
        Dictionary<string, string> OnceTable { get; }
    }
}