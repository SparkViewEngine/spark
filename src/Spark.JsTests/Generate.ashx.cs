using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using Spark.FileSystem;

namespace Spark.JsTests
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Generate : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var engine = new SparkViewEngine
                             {
                                 ViewFolder = new VirtualPathProviderViewFolder("~/Views")
                             };
            var entry = engine.CreateEntry(new SparkViewDescriptor()
                                               .SetLanguage(LanguageType.Javascript)
                                               .AddTemplate(context.Request.PathInfo.TrimStart('/', '\\') + ".spark"));

            //Spark.Simple._LiteralHtml({foo:'asoi'})
            context.Response.ContentType = "text/javascript";
            context.Response.Write(entry.SourceCode);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
