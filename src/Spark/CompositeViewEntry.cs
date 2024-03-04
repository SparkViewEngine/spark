using System;
using System.Collections.Generic;
using System.Diagnostics;
using Spark.Compiler;
using Spark.Parser;

namespace Spark
{
    [DebuggerDisplay("{ViewId}")]
    public class CompositeViewEntry : ISparkViewEntry
    {
        public Guid ViewId { get; set; } = Guid.NewGuid();

        public SparkViewDescriptor Descriptor { get; set; }
        public ViewLoader Loader { get; set; }
        public ViewCompiler Compiler { get; set; }
        public IViewActivator Activator { get; set; }
        public ISparkLanguageFactory LanguageFactory { get; set; }

        public ISparkView CreateInstance()
        {
            throw new System.NotImplementedException();
        }

        public void ReleaseInstance(ISparkView view)
        {
            throw new System.NotImplementedException();
        }

        public bool IsCurrent()
        {
            throw new System.NotImplementedException();
        }

        public string SourceCode => throw new System.NotImplementedException();

        public IList<SourceMapping> SourceMappings => throw new System.NotImplementedException();
    }
}
