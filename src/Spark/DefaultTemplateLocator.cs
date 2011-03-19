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
using System.IO;

namespace Spark
{   	
	public class DefaultTemplateLocator : ITemplateLocator
    {
        public LocateResult LocateMasterFile(IViewFolder viewFolder, string masterName)
        {
			var masterFile = masterName + Constants.DotSpark;
			
			var layoutsMaster = Path.Combine(Constants.Layouts, masterFile);
            if (viewFolder.HasView(layoutsMaster))
            {
                return Result(viewFolder, layoutsMaster);
            }
			
			var sharedMaster = Path.Combine(Constants.Shared, masterFile);
            if (viewFolder.HasView(sharedMaster))
            {
                return Result(viewFolder, sharedMaster);
            }
			
            return new LocateResult 
			{ 
				SearchedLocations = new[] { layoutsMaster, sharedMaster } 
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
