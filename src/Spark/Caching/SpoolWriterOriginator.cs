using System.Linq;
using Spark.Spool;

namespace Spark.Caching
{
    public class SpoolWriterOriginator(SpoolWriter writer) : TextWriterOriginator
    {
        private int _priorStringCount;

        public override TextWriterMemento CreateMemento()
        {
            return new TextWriterMemento { Written = writer.ToArray() };
        }

        public override void BeginMemento()
        {
            _priorStringCount = writer.Count();
        }

        public override TextWriterMemento EndMemento()
        {
            return new TextWriterMemento { Written = writer.Skip(_priorStringCount).ToArray() };
        }

        public override void DoMemento(TextWriterMemento memento)
        {
            foreach (var written in memento.Written)
                writer.Write(written);
        }
    }
}