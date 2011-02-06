// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
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
                                               .AddTemplate(context.Request.PathInfo.TrimStart('/', Path.DirectorySeparatorChar) + ".spark"));

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
