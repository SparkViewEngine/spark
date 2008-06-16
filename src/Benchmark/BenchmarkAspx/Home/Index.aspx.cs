using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using Benchmark;
using Benchmark.Models;

namespace BenchmarkAspx.Home
{
    public partial class Index : System.Web.UI.Page
    {
        static BlogDao dao = new BlogDao();
        private Post post;

        public Post Post
        {
            get { return post; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            post = dao.GetPost();
        }
    }
}
