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

namespace Spark.FileSystem
{
    public static class ViewFolderExtensions
    {
        public static IViewFolder AddSharedPath(this IViewFolder viewFolder, string virtualPath)
        {
            var vppFolder = new SubViewFolder(new VirtualPathProviderViewFolder(virtualPath), "Shared");
            return viewFolder.Append(vppFolder);
        }

        public static IViewFolder AddLayoutsPath(this IViewFolder viewFolder, string virtualPath)
        {
            var vppFolder = new SubViewFolder(new VirtualPathProviderViewFolder(virtualPath), "Layouts");
            return viewFolder.Append(vppFolder);
        }
    }
}
