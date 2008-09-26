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
using System.Data;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace AdvancedPartials.Models
{
    public class Bird
    {
        public string Name { get; set; }
        public string State { get; set; }
    }

    public class BirdRepository
    {
        public IList<Bird> GetBirds()
        {
            using(var stream = typeof (BirdRepository).Assembly.GetManifestResourceStream(typeof (BirdRepository), "BirdList.txt"))
            {
                if (stream != null)
                    using(var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd()
                            .Split("\r\n".ToArray(), StringSplitOptions.RemoveEmptyEntries)
                            .Select(row => row.Split('-'))
                            .Select(parts => new Bird {Name = parts[1].Trim(), State = parts[0].Trim()})
                            .ToList();
                    }
            }
            return null;
        }
    }

}
