using System;
using System.Collections.Generic;
using System.IO;
using Spark.Spool;

namespace Spark.Caching
{
    public abstract class TextWriterOriginator
    {
        public static TextWriterOriginator Create(TextWriter writer)
        {
            if (writer is SpoolWriter)
                return new SpoolWriterOriginator((SpoolWriter) writer);
            if (writer is StringWriter)
                return new StringWriterOriginator((StringWriter)writer);
            throw new InvalidCastException("writer is unknown type " + writer.GetType().FullName);
        }

        public abstract TextWriterMemento CreateMemento();
        public abstract void BeginMemento();
        public abstract TextWriterMemento EndMemento();

        public abstract void DoMemento(TextWriterMemento memento);
    }

    public class TextWriterMemento
    {
        public IEnumerable<string> Written { get; set; }
    }
}