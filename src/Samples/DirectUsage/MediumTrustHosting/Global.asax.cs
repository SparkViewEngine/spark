using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml.Linq;
using Spark;
using Spark.FileSystem;

namespace MediumTrustHosting
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var engine = new SparkViewEngine
                             {
                                 ViewFolder = new FileSystemViewFolder(Server.MapPath("~/Views"))
                             };

            Application["ViewEngine"] = engine;

            engine.LoadBatchCompilation(Assembly.Load("MediumTrustHosting.Views"));
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var path = Request.AppRelativeCurrentExecutionFilePath;
            if (string.Equals(path, "~/default.aspx", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(path, "~/"))
            {
                Context.RewritePath("~/Home.ashx");
            }
        }

        public static IList<SparkViewDescriptor> AllKnownDescriptors()
        {
            return new[]
                       {
                           Desc("home.spark", "master.spark"),
                           Desc("product.spark", "master.spark"),
                           Desc("productlist.spark", "master.spark"),
                       };
        }

        static SparkViewDescriptor Desc(params string[] templates)
        {
            var desc = new SparkViewDescriptor();
            foreach (var template in templates)
                desc.AddTemplate(template);
            return desc;
        }

    }
}