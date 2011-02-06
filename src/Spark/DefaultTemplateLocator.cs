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
using System.IO;
using Spark.FileSystem;

namespace Spark
{
    public class DefaultTemplateLocator : ITemplateLocator
    {
        #region ITemplateLocator Members

        public LocateResult LocateMasterFile(IViewFolder viewFolder, string masterName)
        {
            if (viewFolder.HasView(string.Format("Layouts{0}{1}.spark", Path.DirectorySeparatorChar, masterName)))
            {
                return Result(viewFolder, string.Format("Layouts{0}{1}.spark", Path.DirectorySeparatorChar, masterName));
            }
            if (viewFolder.HasView(string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar, masterName)))
            {
                return Result(viewFolder, string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar, masterName));
            }
            return new LocateResult
                       {
                           SearchedLocations =
                               new[]
                                   {
                                       string.Format("Layouts{0}{1}.spark", Path.DirectorySeparatorChar, masterName),
                                       string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar, masterName)
                                   }
                       };
        }

        #endregion

        private static LocateResult Result(IViewFolder viewFolder, string path)
        {
            return new LocateResult
                       {
                           Path = path,
                           ViewFile = viewFolder.GetViewSource(path)
                       };
        }
    }
}