using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Spark;

namespace MediumTrustHosting
{
    /// <summary>
    /// This provides some base functionality for the classes
    /// in the project that receive .ashx requests
    /// </summary>
    public abstract class BaseHandler : IHttpHandler
    {
        /// <summary>
        /// Assigned by the IHttpHandler.ProcessRequest method
        /// </summary>
        public HttpContext Context { get; set; }


        /// <summary>
        /// Called by the asp.net framework when a request arrives
        /// </summary>
        /// <param name="context"></param>
        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            Context = context;
            Process();
        }

        bool IHttpHandler.IsReusable { get { return false; } }

        /// <summary>
        /// This method is implemented by the various handler classes
        /// </summary>
        public abstract void Process();

        /// <summary>
        /// Called by handler classes to instantiate a view.
        /// </summary>
        /// <param name="templates"></param>
        /// <returns></returns>
        public BaseView CreateView(params string[] templates)
        {
            // the engine is created in Global.Application_Start
            var viewEngine = (ISparkViewEngine)Context.Application["ViewEngine"];

            // a descriptor is used to "key" the type of view to instantiate
            var descriptor = new SparkViewDescriptor();
            foreach (var template in templates)
                descriptor.AddTemplate(template);

            // create the view to return and provides the http context
            var view = (BaseView)viewEngine.CreateInstance(descriptor);
            view.Context = Context;
            return view;
        }

    }
}
