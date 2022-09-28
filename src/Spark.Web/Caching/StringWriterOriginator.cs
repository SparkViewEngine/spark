using System;
using System.IO;

namespace Spark.Caching
{
    public class StringWriterOriginator : TextWriterOriginator
    {
        private readonly StringWriter _writer;
        private int _priorLength;

        public StringWriterOriginator(StringWriter writer)
        {
            _writer = writer;
        }

        public override TextWriterMemento CreateMemento()
        {
            return new TextWriterMemento {Written = new[] {_writer.ToString()}};
        }

        public override void BeginMemento()
        {
            _priorLength = _writer.GetStringBuilder().Length;
        }

        public override TextWriterMemento EndMemento()
        {
            var currentLength = _writer.GetStringBuilder().Length;
            var written = _writer.GetStringBuilder().ToString(_priorLength, currentLength - _priorLength);
            return new TextWriterMemento { Written = new[] { written } };
        }

        public override void DoMemento(TextWriterMemento memento)
        {
            foreach(var written in memento.Written)
                _writer.Write(written);
        }
    }
}
