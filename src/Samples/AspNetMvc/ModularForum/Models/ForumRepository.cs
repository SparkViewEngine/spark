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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace ModularForum.Models
{
    [DataContract]
    public class Forum
    {
        [DataMember]
        public string Id { get; set; }
        
        [DataMember]
        public string Name { get; set; }
        
        [DataMember]
        public string Description { get; set; }
    }

    [DataContract]
    public class ForumData
    {
        [DataMember]
        public List<Forum> Forums { get; set; }
    }

    public class ForumRepository
    {
        public IList<Forum> ListForums()
        {
            ForumData data = LocateData();
            return data.Forums;
        }

        private static ForumData LocateData()
        {
            var data = (ForumData) AppDomain.CurrentDomain.GetData("__ForumData");
            if (data == null)
            {
                var appData = (string) AppDomain.CurrentDomain.GetData("DataDirectory");
                var serializer = new DataContractSerializer(typeof (ForumData));

                using (var stream = new FileStream(Path.Combine(appData, "ForumData.xml"), FileMode.Open,
                                                   FileAccess.Read, FileShare.ReadWrite))
                {
                    data = (ForumData) serializer.ReadObject(stream);
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