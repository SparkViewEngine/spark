using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IronPythonViews.Models
{
    public class BirdRepository
    {
        public IList<Bird> GetBirds()
        {
            using (var stream = typeof (BirdRepository).Assembly.GetManifestResourceStream(typeof (BirdRepository),
                                                                                           "BirdList.txt"))
            {
                if (stream != null)
                    using (var reader = new StreamReader(stream))
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