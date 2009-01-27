using System;
using System.Collections.Generic;
using System.Linq;
using Spark.Compiler;
using Spark.Compiler.ChunkVisitors;
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

        class UseMasterVisitor : ChunkVisitor
        {
            public UseMasterChunk Chunk { get; set; }

            protected override void Visit(UseMasterChunk chunk)
            {
                if (Chunk == null)
                    Chunk = chunk;
            }
        }

        public string UseMaster
        {
            get
            {
                var chunks = Loader.GetEverythingLoaded().FirstOrDefault();
                if (chunks == null)
                    return null;

                var useMaster = new UseMasterVisitor();
                useMaster.Accept(chunks);
                if (useMaster.Chunk == null)
                    return null;

                return useMaster.Chunk.Name;
            }
        }

        public ISparkView CreateInstance()
        {
            return CreateInstance(null);
        }

        public ISparkView CreateInstance(ISparkView decorated)
        {
            var view = Activator.Activate(Compiler.CompiledType, decorated);
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

        //TODO: refactor see #82 
        public bool IsCurrent { get { return Loader.IsCurrent(); } }
    }
}