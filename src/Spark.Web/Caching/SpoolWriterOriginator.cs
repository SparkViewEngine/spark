using System;
using System.Linq;
using Spark.Spool;

namespace Spark.Caching
{
    public class SpoolWriterOriginator : TextWriterOriginator
    {
        private readonly SpoolWriter _writer;
        private int _priorStringCount;

        public SpoolWriterOriginator(SpoolWriter writer)
        {
            _writer = writer;
        }

        public override TextWriterMemento CreateMemento()
        {
            return new TextWriterMemento { Written = _writer.ToArray() };
        }

        public override void BeginMemento()
        {
            _priorStringCount = _writer.Count();
        }

        public override TextWriterMemento EndMemento()
        {
            return new TextWriterMemento { Written = _writer.Skip(_priorStringCount).ToArray() };
        }

        public override void DoMemento(TextWriterMemento memento)
        {
            foreach (var written in memento.Written)
                _writer.Write(written);
        }
    }
}