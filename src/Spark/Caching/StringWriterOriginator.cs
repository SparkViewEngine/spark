using System.IO;

namespace Spark.Caching
{
    public class StringWriterOriginator(StringWriter writer) : TextWriterOriginator
    {
        private int _priorLength;

        public override TextWriterMemento CreateMemento()
        {
            return new TextWriterMemento {Written = new[] {writer.ToString()}};
        }

        public override void BeginMemento()
        {
            _priorLength = writer.GetStringBuilder().Length;
        }

        public override TextWriterMemento EndMemento()
        {
            var currentLength = writer.GetStringBuilder().Length;
            var written = writer.GetStringBuilder().ToString(_priorLength, currentLength - _priorLength);
            return new TextWriterMemento { Written = new[] { written } };
        }

        public override void DoMemento(TextWriterMemento memento)
        {
            foreach(var written in memento.Written)
                writer.Write(written);
        }
    }
}
