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
            _subFolder = subFolder;
        }

        private string Adjust(string path)
        {
            if (!path.StartsWith(_subFolder, StringComparison.InvariantCultureIgnoreCase))
                return null;

            if (path.Length == _subFolder.Length)
                return "";

            if (path[_subFolder.Length] != '/' && path[_subFolder.Length] != '\\')
                return null;

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

            return _viewFolder.ListViews(adjusted).Select(file => _subFolder + "\\" + Path.GetFileName(file)).ToArray();
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
