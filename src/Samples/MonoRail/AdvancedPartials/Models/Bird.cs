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
