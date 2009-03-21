// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
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
using System.Linq;
using System.Text;
using Spark.FileSystem;

namespace Spark.Web.Mvc.Wrappers
{
    public class ViewFolderWrapper : IViewFolder
    {
        private readonly IViewFolderContainer _container;

        public ViewFolderWrapper(IViewFolderContainer container)
        {
            _container = container;
        }

        public IViewFile GetViewSource(string path)
        {
            return _container.ViewFolder.GetViewSource(path);
        }

        public IList<string> ListViews(string path)
        {
            return _container.ViewFolder.ListViews(path);
        }

        public bool HasView(string path)
        {
            return _container.ViewFolder.HasView(path);
        }
    }
}
