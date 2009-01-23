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
        public CompositeViewEntry(SparkViewDescriptor descriptor, List<ISparkViewEntry> compiledEntries)
        {
            ViewId = Guid.NewGuid();
            Descriptor = descriptor;
            CompiledEntries = compiledEntries;
        }
    
        public Guid ViewId { get; set; }
        public SparkViewDescriptor Descriptor { get; set; }
        public List<ISparkViewEntry> CompiledEntries { get; private set; }
         

        public ISparkView CreateInstance()
        {
            return CreateInstance(null);
        }

        public ISparkView CreateInstance(ISparkView decorated)
        {
            var result = decorated;
            foreach(var entry in CompiledEntries)
                result = entry.CreateInstance(result);

            return result;
        }

        public void ReleaseInstance(ISparkView view)
        {
            var count = CompiledEntries.Count;

            var views = new List<ISparkView>(CompiledEntries.Count);

            var decorator = view as ISparkViewDecorator;
            while(decorator != null)
            {
                views.Add(decorator);
                decorator = decorator.Decorated as ISparkViewDecorator;
            }

            if (views.Count != count)
                throw new IndexOutOfRangeException("Incorrect number of views released");

            for(var index = 0; index != count; ++index)
            {
                CompiledEntries[index].ReleaseInstance(views[count - index - 1]);
            }
        }

        public bool IsCurrent
        {
            get { return CompiledEntries.All(e => e.IsCurrent); }
        }

        public string SourceCode
        {
            get { throw new System.NotImplementedException(); }
        }

        public IList<SourceMapping> SourceMappings
        {
            get { return CompiledEntries.SelectMany(e => e.SourceMappings).ToList(); }
        }
    }
}
