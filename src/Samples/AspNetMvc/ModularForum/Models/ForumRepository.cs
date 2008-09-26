using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace ModularForum.Models
{
    public class Forum
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ForumData
    {
        public List<Forum> Forums { get; set; }
    }

    public class ForumRepository
    {
        public IList<Forum> ListForums()
        {
            var data = LocateData();
            return data.Forums;
        }

        private static ForumData LocateData()
        {
            var data = (ForumData)AppDomain.CurrentDomain.GetData("__ForumData");
            if (data == null)
            {
                var appData = (string)AppDomain.CurrentDomain.GetData("DataDirectory");
                var serializer = new DataContractSerializer(typeof(ForumData));

                using (var stream = new FileStream(Path.Combine(appData, "ForumData.xml"), FileMode.Open,
                                                FileAccess.Read, FileShare.ReadWrite))
                {
                    data = (ForumData)serializer.ReadObject(stream);
                }
                AppDomain.CurrentDomain.SetData("__ForumData", data);
            }
            return data;
        }

        public Forum GetForum(string id)
        {
            return LocateData().Forums.First(f => f.Id == id);
        }
    }
}
