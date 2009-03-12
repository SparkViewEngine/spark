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
using System.Linq;
using System.Text;
using Spark.FileSystem;

namespace Castle.MonoRail.Views.Spark.Wrappers
{
    public class ViewSourceLoaderWrapper : IViewFolder
    {
        private readonly IViewSourceLoaderContainer _container;

        public ViewSourceLoaderWrapper(IViewSourceLoaderContainer container)
        {
            _container = container;
        }

        public IViewFile GetViewSource(string path)
        {
            var viewSource = _container.ViewSourceLoader.GetViewSource(Path.ChangeExtension(path, ".spark"));
            if(viewSource==null)
            {
              throw new FileNotFoundException(string.Format("Template {0} not found", path), path);
            }
            return new ViewSourceWrapper(viewSource);
        }

        public IList<string> ListViews(string path)
        {
            return _container.ViewSourceLoader.ListViews(path);
        }

        public bool HasView(string path)
        {
            return _container.ViewSourceLoader.HasSource(path);
        }
    }
}
