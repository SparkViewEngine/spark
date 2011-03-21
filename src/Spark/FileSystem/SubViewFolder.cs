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
using System.IO;
using System.Linq;
using System.Text;

namespace Spark.FileSystem
{
    /// <summary>
    /// This view folder is for targetting the subdirectory of a request as
    /// a higher director in the actual viewfolder.
    /// 
    /// An example would be to take a vpp dir "~/MoreShared" and put it under a
    /// subfolder "Shared". 
    /// 
    /// This way "~/MoreShared/x" will be matched against path "Shared/x"
    /// </summary>
	
	public class SubViewFolder : IViewFolder
    {
        private readonly IViewFolder _viewFolder;
        private readonly string _subFolder;

        public SubViewFolder(IViewFolder viewFolder, string subFolder)
        {
            _viewFolder = viewFolder;
            _subFolder = subFolder.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        private string Adjust(string path)
        {
            if (!path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).StartsWith(_subFolder, StringComparison.InvariantCultureIgnoreCase))
                return null;

            if (path.Length == _subFolder.Length)
                return string.Empty;

            if (path[_subFolder.Length] != Path.AltDirectorySeparatorChar &&
                path[_subFolder.Length] != Path.DirectorySeparatorChar)
            {
                return null;
            }

            return path.Substring(_subFolder.Length + 1);
        }

        public IViewFile GetViewSource(string path)
        {
            var adjusted = Adjust(path);
            if (adjusted == null)
                throw new FileNotFoundException("File not found", path);
            return _viewFolder.GetViewSource(adjusted);
        }


        public IList<string> ListViews(string path)
        {
            var adjusted = Adjust(path);
            if (adjusted == null)
                return new string[0];

            return _viewFolder.ListViews(adjusted).Select(file => Path.Combine(_subFolder, Path.GetFileName(file))).ToArray();
        }

        public bool HasView(string path)
        {
            var adjusted = Adjust(path);
            if (adjusted == null)
                return false;

            return _viewFolder.HasView(adjusted);
        }
    }
}
