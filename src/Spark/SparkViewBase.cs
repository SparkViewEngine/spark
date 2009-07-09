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
using Spark.Spool;

namespace Spark
{
    public class SparkViewContext
    {
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
                       Interlocked.CompareExchange(ref _sparkViewContext, CreateSparkViewContext(), null) ?? 
                       _sparkViewContext;
            }
            set { _sparkViewContext = value; }
        }

        private static SparkViewContext CreateSparkViewContext()
        {
            return new SparkViewContext
                   {
                       Content = new Dictionary<string, TextWriter>(),
                       Globals = new Dictionary<string, object>(),
                       OnceTable = new Dictionary<string, string>()
                   };
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

        public IDisposable CacheScope(string site, string key)
        {
            return new CacheScopeImpl(this, site, key);
        }

        //todo: fix for decorator pattern context?
        public CacheScopeImpl CacheContext { get; set; }
        public ICacheService CacheService { get; set; }

        public class CacheScopeImpl : IDisposable
        {
            private readonly SparkViewBase _view;
            private readonly string _site;
            private readonly string _key;
            private readonly CacheScopeImpl _previous;

            private TextWriter _output;
            private IDisposable _outputScope;

            private static readonly ICacheService _nullCacheService = new NullCacheService();


            public CacheScopeImpl(SparkViewBase view, string site, string key)
            {
                _view = view;
                _site = site;
                _key = key;
                _previous = _view.CacheContext;
                _view.CacheContext = this;
            }

            public void Dispose()
            {
                _view.CacheContext = _previous;
            }

            public bool Begin()
            {
                var cacheItem = (StringWriter)(_view.CacheService ?? _nullCacheService).Get(_site + _key);
                if (cacheItem == null)
                {
                    _output = new StringWriter();
                    _outputScope = _view.OutputScope(_output);
                    return true;
                }

                cacheItem.WriteTo(_view.Output);
                return false;
            }

            public void End()
            {
                _outputScope.Dispose();
                _outputScope = null;

                _output.WriteTo(_view.Output);
                (_view.CacheService ?? _nullCacheService).Store(_site + _key, _output);
            }


            private class NullCacheService : ICacheService
            {
                public object Get(string identifier)
                {
                    return null;
                }

                public void Store(string identifier, object item)
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
