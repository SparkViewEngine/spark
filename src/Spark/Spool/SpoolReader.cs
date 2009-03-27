using System.Collections.Generic;
using System.IO;

namespace Spark.Spool
{
    public class SpoolReader : TextReader
    {
        readonly IEnumerator<string> _enumerator;
        
        private string _cursorData;
        private int _cursorIndex;

        public SpoolReader(IEnumerable<string> source)
        {
            _enumerator = source.GetEnumerator();
        }

        bool EnsureCursor()
        {
            while (_cursorData == null || _cursorIndex == _cursorData.Length)
            {
                if (!AdvanceCursor())
                    return false;
            }

            return true;
        }

        private bool AdvanceCursor()
        {
            _cursorIndex = 0;

            var hasNext = _enumerator.MoveNext();
            _cursorData = hasNext ? _enumerator.Current : null;
            return hasNext;
        }

        public override int Peek()
        {
            return EnsureCursor() ? _cursorData[_cursorIndex] : -1;
        }

        public override int Read()
        {
            var ch = Peek();
            if (ch != -1)
                ++_cursorIndex;
            return ch;
        }

        //TODO: override read byte[] for efficiency
    }
}