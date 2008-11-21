using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.FileSystem;

namespace Spark
{
    public class DefaultTemplateLocator : ITemplateLocator
    {
        public LocateResult LocateMasterFile(IViewFolder viewFolder, string masterName)
        {
            if (viewFolder.HasView("Layouts\\" + masterName + ".spark"))
            {
                return Result(viewFolder, "Layouts\\" + masterName + ".spark");
            }
            if (viewFolder.HasView("Shared\\" + masterName + ".spark"))
            {
                return Result(viewFolder, "Shared\\" + masterName + ".spark");
            }
            return new LocateResult
                   {
                       SearchedLocations = new[]
                                           {
                                               "Layouts\\" + masterName + ".spark", 
                                               "Shared\\" + masterName + ".spark"
                                           }
                   };
        }


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
