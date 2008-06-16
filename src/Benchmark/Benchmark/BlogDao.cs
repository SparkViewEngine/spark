using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Benchmark.Models;

namespace Benchmark
{
    public class BlogDao
    {
        private static readonly Post _post = BuildReferencePost();


        public Post GetPost()
        {
            return _post;
        }

        private static Post BuildReferencePost()
        {
            var ser = new XmlSerializer(typeof (Post));
            var type = typeof(BlogDao);
            using (Stream stream = type.Assembly.GetManifestResourceStream(type, "ReferenceData.xml"))
            {
                if (stream == null)
                    throw new ApplicationException("ReferenceData.xml resource not available");
                return (Post) ser.Deserialize(stream);
            }
        }
    }
}
