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
    public class SparkContext<TExtended> 
    {
        public TextWriter Output { get; set; }
        public Dictionary<string, TextWriter> Content { get; set; }
        public Dictionary<string, object> Globals { get; set; }
        public Dictionary<string, string> OnceTable { get; set; }
        public TExtended Extended { get; set; }
    }

    public abstract class SparkViewBase<TExtended> : ISparkView where TExtended : new()
    {
        private SparkContext<TExtended> _sparkContext;

        public abstract Guid GeneratedViewId { get; }

        public virtual bool TryGetViewData(string name, out object value)
        {
            value = null;
            return false;
        }

        public virtual SparkContext<TExtended> SparkContext
        {
            get
            {
                return _sparkContext ??
                       Interlocked.CompareExchange(ref _sparkContext, CreateSparkContext(), null) ??
                       _sparkContext;
            }
            set { _sparkContext = value; }
        }

        private static SparkContext<TExtended> CreateSparkContext()
        {
            return new SparkContext<TExtended>
                   {
                       Content = new Dictionary<string, TextWriter>(),
                       Globals = new Dictionary<string, object>(),
                       OnceTable = new Dictionary<string, string>(),
                       Extended = new TExtended()
                   };
        }

        public TextWriter Output { get { return SparkContext.Output; } set { SparkContext.Output = value; } }
        public Dictionary<string, TextWriter> Content { get { return SparkContext.Content; } set { SparkContext.Content = value; } }
        public Dictionary<string, object> Globals { get { return SparkContext.Globals; } set { SparkContext.Globals = value; } }
        public Dictionary<string, string> OnceTable { get { return SparkContext.OnceTable; } set { SparkContext.OnceTable = value; } }

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
            if (SparkContext.OnceTable.ContainsKey(flagString))
                return false;

            SparkContext.OnceTable.Add(flagString, null);
            return true;
        }


        public class OutputScopeImpl : IDisposable
        {
            private readonly SparkViewBase<TExtended> view;
            private readonly TextWriter previous;

            public OutputScopeImpl(SparkViewBase<TExtended> view, TextWriter writer)
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
