// Copyright 2008-2024 Louis DeJardin
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;
using System.Collections.Generic;
using System.Threading;

namespace Spark.Spool
{
    public class SpoolPage
    {
        public const int BUFFER_SIZE = 250;
        static readonly Allocator _allocator = new Allocator();

        private readonly string[] _buffer;
        private int _count;
        private SpoolPage _next;
        private bool _readonly;
        private bool _nonreusable;
		
		private bool _released;
		
        public SpoolPage()
        {
            _buffer = _allocator.Obtain();
        }

        private SpoolPage(SpoolPage original)
        {
            _buffer = original._buffer;
            _count = original._count;
            if (original._next != null)
                _next = new SpoolPage(original._next);

            _readonly = true;
            original._readonly = true;
            _nonreusable = true;
            original._nonreusable = true;
        }
        

        public SpoolPage Next
        {
            get { return _next; }
        }

        public int Count
        {
            get { return _count; }
        }

        public string[] Buffer
        {
            get { return _buffer; }
        }

        public SpoolPage Append(SpoolPage pages)
        {
            _readonly = true;
            _next = new SpoolPage(pages);
            var scan = _next;
            while(scan._next != null)
                scan = scan._next;
            return scan;
        }

        public SpoolPage Append(string value)
        {
            if (_readonly)
            {
                if (_next == null)
                    _next = new SpoolPage();
                return _next.Append(value);
            }

            _buffer[_count++] = value;
            if (_count == BUFFER_SIZE)
                _readonly = true;
            return this;
        }

        public void Release()
        {
            if (!Monitor.TryEnter(_allocator._cache))
                return;

            try
            {
                var scan = this;
                while (scan != null && _allocator._cache.Count < 200)
                {
                    if (!scan._nonreusable && !scan._released)
                    {
						scan._released = true;
                        Array.Clear(scan._buffer, 0, scan._count);
                        _allocator._cache.Push(scan._buffer);
                    }
                    scan = scan._next;
                }
            }
            finally
            {
                Monitor.Exit(_allocator._cache);
            }
        }


        class Allocator
        {
            internal readonly Stack<string[]> _cache = new Stack<string[]>();

            public string[] Obtain()
            {
                if (!Monitor.TryEnter(_cache))
                    return new string[BUFFER_SIZE];

                try
                {
                    return _cache.Count != 0 ? _cache.Pop() : new string[BUFFER_SIZE];
                }
                finally
                {
                    Monitor.Exit(_cache);
                }
            }
        }
    }
}