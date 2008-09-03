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
using System.Collections.Generic;
using System.IO;
using MvcContrib.ViewFactories;
using Spark.FileSystem;

namespace MvcContrib.SparkViewEngine
{
    public class ViewSourceLoaderWrapper : IViewFolder
    {
        public ViewSourceLoaderWrapper(IViewSourceLoader viewSourceLoader)
        {
            ViewSourceLoader = viewSourceLoader;
        }

        public IViewSourceLoader ViewSourceLoader { get; set; }

        #region IViewFolder Members

        public bool HasView(string path)
        {
            return ViewSourceLoader.HasView(path);
        }

        public IList<string> ListViews(string path)
        {
            return ViewSourceLoader.ListViews(path);
        }

        public IViewFile GetViewSource(string path)
        {
            return new ViewSourceWrapper(ViewSourceLoader.GetViewSource(path));
        }

        #endregion

        #region Nested type: ViewSourceWrapper

        internal class ViewSourceWrapper : IViewFile
        {
            private readonly IViewSource _source;

            public ViewSourceWrapper(IViewSource source)
            {
                _source = source;
            }

            #region IViewFile Members

            public long LastModified
            {
                get { return _source.LastModified; }
            }

            public Stream OpenViewStream()
            {
                return _source.OpenViewStream();
            }

            #endregion
        }

        #endregion
    }
}