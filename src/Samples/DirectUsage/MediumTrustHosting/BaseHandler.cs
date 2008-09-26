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
using System.Web;
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

        #region IHttpHandler Members

        /// <summary>
        /// Called by the asp.net framework when a request arrives
        /// </summary>
        /// <param name="context"></param>
        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            Context = context;
            Process();
        }

        bool IHttpHandler.IsReusable
        {
            get { return false; }
        }

        #endregion

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
            var viewEngine = (ISparkViewEngine) Context.Application["ViewEngine"];

            // a descriptor is used to "key" the type of view to instantiate
            var descriptor = new SparkViewDescriptor();
            foreach (string template in templates)
                descriptor.AddTemplate(template);

            // create the view to return and provides the http context
            var view = (BaseView) viewEngine.CreateInstance(descriptor);
            view.Context = Context;
            return view;
        }
    }
}