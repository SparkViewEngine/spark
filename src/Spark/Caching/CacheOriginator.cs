using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spark.Spool;

namespace Spark.Caching
{
    public class CacheOriginator
    {
        private readonly ICacheSubject _subject;

        private TextWriter _priorOutput;
        private SpoolWriter _spoolOutput;

        private readonly Dictionary<string, TextWriterOriginator> _priorContent = new Dictionary<string, TextWriterOriginator>();

        public CacheOriginator(ICacheSubject subject)
        {
            _subject = subject;
        }

        /// <summary>
        /// Establishes original state for memento capturing purposes
        /// </summary>
        public void BeginMemento()
        {
            _priorOutput = _subject.Output;
            _spoolOutput = new SpoolWriter();
            _subject.Output = _spoolOutput;

            //TODO: don't capture output if it's also one of the content streams
            foreach (var content in _subject.Content)
            {
                var writerOriginator = TextWriterOriginator.Create(content.Value);
                _priorContent.Add(content.Key, writerOriginator);
                writerOriginator.BeginMemento();
            }
        }

        /// <summary>
        /// Finalizes state change and creates memento 
        /// </summary>
        /// <returns>memento holding the details of the resulting state delta</returns>
        public CacheMemento EndMemento()
        {
            _subject.Output = _priorOutput;
            _spoolOutput.WriteTo(_subject.Output);

            var memento = new CacheMemento(_spoolOutput);
            foreach (var content in _priorContent)
            {
                memento.Content.Add(content.Key, content.Value.EndMemento());
            }
            foreach (var content in _subject.Content.Where(kv => _priorContent.ContainsKey(kv.Key) == false))
            {
                var originator = TextWriterOriginator.Create(content.Value);
                memento.Content.Add(content.Key, originator.CreateMemento());
            }
            return memento;
        }

        /// <summary>
        /// Applies previously captured state delta to view, effectively replaying cached fragment
        /// </summary>
        /// <param name="memento">memento captured in previous begin/end calls</param>
        public void DoMemento(CacheMemento memento)
        {
            memento.SpoolOutput.WriteTo(_subject.Output);

            foreach(var content in memento.Content)
            {
                // create named content if it doesn't exist
                TextWriter writer;
                if (_subject.Content.TryGetValue(content.Key, out writer) == false)
                {
                    writer = new SpoolWriter();
                    _subject.Content.Add(content.Key, writer);
                }

                // and in any case apply the delta
                var originator = TextWriterOriginator.Create(writer);
                originator.DoMemento(content.Value);
            }
        }
    }
}
