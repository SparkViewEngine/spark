using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReferenceSite.Model
{
    public class Story
    {
        public int Id { get; set; }

        public string Link { get; set; }
        public long SubmitDate { get; set; }
        public int Diggs { get; set; }
        public int Comments { get; set; }
        public string Href { get; set; }
        public string Status { get; set; }
        public string Media { get; set; }

        public string Description { get; set; }
        public string Title { get; set; }

        public User User { get; set; }
        public Thumbnail Thumbnail { get; set; }
    }

    public class User
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public int? Registered { get; set; }
        public int? Profileviews { get; set; }
        public string Icon { get; set; }
    }

    public class Thumbnail
    {
        public string ContentType { get; set; }
        public string Src { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }
    }
}
