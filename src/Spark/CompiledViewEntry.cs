using System;
using System.Collections.Generic;
using Spark.Compiler;
using Spark.Parser;

namespace Spark
{
    public class CompiledViewEntry : ISparkViewEntry
    {
        public Guid ViewId { get { return Compiler.GeneratedViewId; } }

        public SparkViewDescriptor Descriptor { get; set; }
        public ViewLoader Loader { get; set; }
        public ViewCompiler Compiler { get; set; }
        public IViewActivator Activator { get; set; }
        public ISparkLanguageFactory LanguageFactory { get; set; }

        public string SourceCode
        {
            get { return Compiler.SourceCode; }
        }

        public IList<SourceMapping> SourceMappings
        {
            get { return Compiler.SourceMappings; }
        }

        public ISparkView CreateInstance()
        {
            var view = Activator.Activate(Compiler.CompiledType);
            if (LanguageFactory != null)
                LanguageFactory.InstanceCreated(Compiler, view);
            return view;
        }

        public void ReleaseInstance(ISparkView view)
        {
            if (LanguageFactory != null)
                LanguageFactory.InstanceReleased(Compiler, view);
            Activator.Release(Compiler.CompiledType, view);
        }

        public bool IsCurrent() { return Loader.IsCurrent(); }
    }
}