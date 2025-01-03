﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spark.Spool;

namespace Spark.Caching
{
    public class CacheOriginator(SparkViewContext state)
    {
        private TextWriter _priorOutput;
        private SpoolWriter _spoolOutput;

        private readonly Dictionary<string, TextWriterOriginator> _priorContent = new();
        private Dictionary<string, string> _priorOnceTable;

        /// <summary>
        /// Establishes original state for memento capturing purposes
        /// </summary>
        public void BeginMemento()
        {
            foreach (var content in state.Content)
            {
                var writerOriginator = TextWriterOriginator.Create(content.Value);
                _priorContent.Add(content.Key, writerOriginator);
                writerOriginator.BeginMemento();
            }

            _priorOnceTable = state.OnceTable.ToDictionary(kv=>kv.Key, kv=>kv.Value);

            // capture current output also if it's not locked into a named output at the moment
            // this could be a case in view's output, direct to network, or various macro or content captures
            if (state.Content.Any(kv => ReferenceEquals(kv.Value, state.Output)) == false)
            {
                _priorOutput = state.Output;
                _spoolOutput = new SpoolWriter();
                state.Output = _spoolOutput;
            }
        }

        /// <summary>
        /// Finalizes state change and creates memento 
        /// </summary>
        /// <returns>memento holding the details of the resulting state delta</returns>
        public CacheMemento EndMemento()
        {
            var memento = new CacheMemento();

            // for capturing subject.Output directly, replay what was spooled, and save it
            // in the memento
            if (_priorOutput != null)
            {
                _spoolOutput.WriteTo(_priorOutput);
                state.Output = _priorOutput;
                memento.SpoolOutput = _spoolOutput;
            }
            
            // save any deltas on named content that have expanded
            foreach (var content in _priorContent)
            {
                var textMemento = content.Value.EndMemento();
                if (textMemento.Written.Any(part=>string.IsNullOrEmpty(part) == false))
                    memento.Content.Add(content.Key, textMemento);
            }

            // also save any named content in its entirety that added created after BeginMemento was called
            foreach (var content in state.Content.Where(kv => _priorContent.ContainsKey(kv.Key) == false))
            {
                var originator = TextWriterOriginator.Create(content.Value);
                memento.Content.Add(content.Key, originator.CreateMemento());
            }

            // capture anything from the oncetable that was added after BeginMemento was called
            var newItems = state.OnceTable.Where(once => _priorOnceTable.ContainsKey(once.Key) == false);
            memento.OnceTable = newItems.ToDictionary(once => once.Key, once => once.Value);
            return memento;
        }

        /// <summary>
        /// Applies previously captured state delta to view, effectively replaying cached fragment
        /// </summary>
        /// <param name="memento">memento captured in previous begin/end calls</param>
        public void DoMemento(CacheMemento memento)
        {
            memento.SpoolOutput.WriteTo(state.Output);

            foreach (var content in memento.Content)
            {
                // create named content if it doesn't exist
                TextWriter writer;
                if (state.Content.TryGetValue(content.Key, out writer) == false)
                {
                    writer = new SpoolWriter();
                    state.Content.Add(content.Key, writer);
                }

                // and in any case apply the delta
                var originator = TextWriterOriginator.Create(writer);
                originator.DoMemento(content.Value);
            }

            // add recorded once deltas that were not yet in this subject's table
            var newItems = memento.OnceTable.Where(once => state.OnceTable.ContainsKey(once.Key) == false);
            foreach (var once in newItems)
            {
                state.OnceTable.Add(once.Key, once.Value);
            }
        }
    }
}
