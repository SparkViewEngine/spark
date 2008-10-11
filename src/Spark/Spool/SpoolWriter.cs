using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spark.Spool
{
    public class SpoolWriter : TextWriter, IEnumerable<string>
    {
        private readonly SpoolPage _first;
        private SpoolPage _last;
        public SpoolWriter()
        {
            _first = new SpoolPage();
            _last = _first;
        }
        
        ~SpoolWriter()
        {
            Dispose(false);
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        public override void Write(char value)
        {
            Write(new string(value, 1));
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Write(new string(buffer, index, count));
        }

        public override void Write(string value)
        {
            _last = _last.Append(value);
        }

        public override void Write(object value)
        {
            if (value is SpoolWriter)
            {
                ((SpoolWriter)value).SendToSpoolWriter(this);
            }
            else
            {
                base.Write(value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _first.Release();            
        }


        public override string ToString()
        {
            return string.Concat(this.ToArray());
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            var scan = _first;
            while (scan != null)
            {
                var buffer = scan.Buffer;
                var count = scan.Count;
                for (var index = 0; index != count; ++index)
                    yield return buffer[index];
                scan = scan.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

        internal void SendToSpoolWriter(SpoolWriter target)
        {
            target._last = target._last.Append(_first);
        }

        internal void SendToTextWriter(TextWriter target)
        {
            var scan = _first;
            while (scan != null)
            {
                var buffer = scan.Buffer;
                var count = scan.Count;
                for (var index = 0; index != count; ++index)
                    target.Write(buffer[index]);
                scan = scan.Next;
            }
        }
    }
}
