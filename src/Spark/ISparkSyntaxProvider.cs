using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Parser;

namespace Spark
{
    public interface ISparkSyntaxProvider
    {
        IList<Chunk> GetChunks(string viewPath, IViewFolder viewFolder, ISparkExtensionFactory extensionFactory, string prefix);
    }
}
