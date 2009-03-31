using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Linq;

namespace ReferenceSite.Model
{
    public class DataContextException : Exception
    {
        public DataContextException()
        {
        }

        public DataContextException(string message)
            : base(message)
        {
        }

        public DataContextException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DataContextException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    public class DataContext
    {
        public void Initialize()
        {
            var document = XDocument.Load(DataFileLocation());
            if (document == null)
                throw new DataContextException("Unable to load data file location");

            // ReSharper disable PossibleNullReferenceException
            var stories = from s in document.Root.Elements("story")
                          select new Story
                                 {
                                     Id = (int)s.Attribute("id"),
                                     Link = (string)s.Attribute("link"),
                                     SubmitDate = (long)s.Attribute("submit_date"),
                                     Diggs = (int)s.Attribute("diggs"),
                                     Comments = (int)s.Attribute("comments"),
                                     Href = (string)s.Attribute("href"),
                                     Status = (string)s.Attribute("status"),
                                     Media = (string)s.Attribute("media"),

                                     Description = (string)s.Element("description"),
                                     Title = (string)s.Element("title"),

                                     User = (from u in s.Elements("user")
                                             select new User
                                                    {
                                                        Name = (string)u.Attribute("name"),
                                                        FullName = (string)u.Attribute("fullname"),
                                                        Registered = (int?)u.Attribute("registered"),
                                                        Profileviews = (int?)u.Attribute("provileviews"),
                                                        Icon = (string)u.Attribute("icon"),
                                                    }).FirstOrDefault(),

                                     Thumbnail = (from t in s.Elements("thumbnail")
                                                  select new Thumbnail
                                                         {
                                                             ContentType = (string)t.Attribute("contenttype"),
                                                             Src = (string)t.Attribute("src"),
                                                             Width = (int)t.Attribute("width"),
                                                             Height = (int)t.Attribute("height"),
                                                             OriginalWidth = (int)t.Attribute("originalwidth"),
                                                             OriginalHeight = (int)t.Attribute("originalheight"),
                                                         }).FirstOrDefault()
                                 };
            // ReSharper restore PossibleNullReferenceException

            Story = stories.ToArray();
        }


        public string DataFileLocation()
        {
            var appData = (string)AppDomain.CurrentDomain.GetData("DataDirectory") ?? AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appData, "StoryData.xml");
        }

        public IEnumerable<Story> Story { get; private set; }

        public IEnumerable<User> User
        {
            get { return Story.Where(s => s.User != null).Select(s => s.User); }
        }
    }
}
