using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Benchmark.Models
{
    public class Post
    {
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public Author Author { get; set; }
        public string Content { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
