using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler;
using Spark.Parser;

namespace Spark
{
    public class CompositeViewEntry : ISparkViewEntry
    {
        public CompositeViewEntry()
        {
            ViewId = Guid.NewGuid();
        }

        public Guid ViewId { get; set; }

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

        public bool IsCurrent
        {
            get { throw new System.NotImplementedException(); }
        }

        public string SourceCode
        {
            get { throw new System.NotImplementedException(); }
        }

        public IList<SourceMapping> SourceMappings
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}
