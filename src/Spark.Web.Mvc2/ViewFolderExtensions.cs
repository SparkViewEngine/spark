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
using System.Reflection;
using Spark.FileSystem;
using Spark.Web.Mvc.Wrappers;

namespace Spark.Web.Mvc
{
    public static class ViewFolderExtensions
    {
        public static void AddSharedPath(this IViewFolderContainer viewFolderContainer, string virtualPath)
        {
            viewFolderContainer.ViewFolder = viewFolderContainer.ViewFolder.AddSharedPath(virtualPath);
        }

        public static void AddLayoutsPath(this IViewFolderContainer viewFolderContainer, string virtualPath)
        {
            viewFolderContainer.ViewFolder = viewFolderContainer.ViewFolder.AddLayoutsPath(virtualPath);
        }

        public static void AddEmbeddedResources(this IViewFolderContainer viewFolderContainer, Assembly assembly, string resourcePath)
        {
            viewFolderContainer.ViewFolder =
                viewFolderContainer.ViewFolder.Append(new EmbeddedViewFolder(assembly, resourcePath));
        }
    }
}
