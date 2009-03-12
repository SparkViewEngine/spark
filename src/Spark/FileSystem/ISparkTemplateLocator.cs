using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.FileSystem;

namespace Spark.FileSystem
{
    public interface ITemplateLocator
    {
        LocateResult LocateMasterFile(IViewFolder viewFolder, string masterName);
    }

    public class LocateResult
    {
        public string Path { get; set; }
        public IViewFile ViewFile { get; set; }
        public IList<string> SearchedLocations { get; set; }
    }
}