// Copyright 2008 Louis DeJardin - http://whereslou.com
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
using System.IO;
using System.Threading;
using Spark.Caching;
using Spark.Spool;
using Spark.Utilities;

namespace Spark
{
    public class SparkViewContext
    {
        public SparkViewContext()
        {
            Content = new Dictionary<string, TextWriter>();
            Globals = new Dictionary<string, object>();
            OnceTable = new Dictionary<string, string>();
        }

        public TextWriter Output { get; set; }
        public Dictionary<string, TextWriter> Content { get; set; }
        public Dictionary<string, object> Globals { get; set; }
        public Dictionary<string, string> OnceTable { get; set; }
    }

    public abstract class SparkViewBase : ISparkView
    {
        private SparkViewContext _sparkViewContext;

        public abstract Guid GeneratedViewId { get; }

        public virtual bool TryGetViewData(string name, out object value)
        {
            value = null;
            return false;
        }

        public virtual SparkViewContext SparkViewContext
        {
            get
            {
                return _sparkViewContext ??
                       Interlocked.CompareExchange(ref _sparkViewContext, new SparkViewContext(), null) ??
                       _sparkViewContext;
            }
            set { _sparkViewContext = value; }
        }


        public TextWriter Output { get { return SparkViewContext.Output; } set { SparkViewContext.Output = value; } }
        public Dictionary<string, TextWriter> Content { get { return SparkViewContext.Content; } set { SparkViewContext.Content = value; } }
        public Dictionary<string, object> Globals { get { return SparkViewContext.Globals; } set { SparkViewContext.Globals = value; } }
        public Dictionary<string, string> OnceTable { get { return SparkViewContext.OnceTable; } set { SparkViewContext.OnceTable = value; } }

        public IDisposable OutputScope(string name)
        {
            TextWriter writer;
            if (!Content.TryGetValue(name, out writer))
            {
                writer = new SpoolWriter();
                Content.Add(name, writer);
            }
            return new OutputScopeImpl(this, writer);
        }

        public IDisposable OutputScope(TextWriter writer)
        {
            return new OutputScopeImpl(this, writer);
        }

        public IDisposable OutputScope()
        {
            return new OutputScopeImpl(this, new SpoolWriter());
        }

        public IDisposable MarkdownOutputScope()
        {
            return new MarkdownOutputScopeImpl(this, new SpoolWriter());
        }


        public bool Once(object flag)
        {
            var flagString = Convert.ToString(flag);
            if (SparkViewContext.OnceTable.ContainsKey(flagString))
                return false;

            SparkViewContext.OnceTable.Add(flagString, null);
            return true;
        }


        public class OutputScopeImpl : IDisposable
        {
            private readonly SparkViewBase view;
            private readonly TextWriter previous;

            public OutputScopeImpl(SparkViewBase view, TextWriter writer)
            {
                this.view = view;
                previous = view.Output;
                view.Output = writer;
            }

            public void Dispose()
            {
                view.Output = previous;
            }
        }

        public class MarkdownOutputScopeImpl : IDisposable
        {
            private readonly SparkViewBase view;
            private readonly TextWriter previous;

            public MarkdownOutputScopeImpl(SparkViewBase view, TextWriter writer)
            {
                this.view = view;
                previous = view.Output;
                view.Output = writer;
            }

            public void Dispose()
            {
                var source = view.Output.ToString();
                view.Output = previous;

                var markdown = new Markdown();

                view.Output.Write(markdown.Transform(source));
            }
        }

        protected bool BeginCachedContent(string site, CacheExpires expires, params object[] key)
        {
            _currentCacheScope = new CacheScopeImpl(this, CacheUtilities.ToIdentifier(site, key), expires);
            if (_currentCacheScope.Begin())
                return true;

            EndCachedContent();
            return false;
        }

        protected void EndCachedContent()
        {
            _currentCacheScope = _currentCacheScope.End(null);
        }

        protected void EndCachedContent(ICacheSignal signal)
        {
            _currentCacheScope = _currentCacheScope.End(signal);
        }

        private CacheScopeImpl _currentCacheScope;
        public ICacheService CacheService { get; set; }


        private class CacheScopeImpl
        {
            private readonly CacheScopeImpl _previousCacheScope;

            private readonly ICacheService _cacheService;
            private readonly CacheOriginator _originator;
            private readonly string _identifier;
            private readonly CacheExpires _expires;
            private bool _recording;

            private static readonly ICacheService _nullCacheService = new NullCacheService();

            public CacheScopeImpl(SparkViewBase view, string identifier, CacheExpires expires)
            {
                _identifier = identifier;
                _expires = expires;
                _previousCacheScope = view._currentCacheScope;
                _cacheService = view.CacheService ?? _nullCacheService;
                _originator = new CacheOriginator(view.SparkViewContext);
            }


            public bool Begin()
            {
                var memento = _cacheService.Get(_identifier) as CacheMemento;
                if (memento == null)
                {
                    _recording = true;
                    _originator.BeginMemento();
                }
                else
                {
                    _recording = false;
                    _originator.DoMemento(memento);
                }

                return _recording;
            }

            public CacheScopeImpl End(ICacheSignal signal)
            {
                if (_recording)
                {
                    var memento = _originator.EndMemento();
                    _cacheService.Store(_identifier, _expires, signal, memento);
                }
                return _previousCacheScope;
            }


            private class NullCacheService : ICacheService
            {
                public object Get(string identifier)
                {
                    return null;
                }

                public void Store(string identifier, CacheExpires expires, ICacheSignal signal, object item)
                {
                }
            }
        }

        public virtual void RenderView(TextWriter writer)
        {
            using (OutputScope(writer))
            {
                Render();
            }
        }

        public abstract void Render();
    }

}
