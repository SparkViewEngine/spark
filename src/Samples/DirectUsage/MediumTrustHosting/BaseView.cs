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
using System.Linq;
using System.Web;
using Spark;

namespace MediumTrustHosting
{
    /// <summary>
    /// Provide a base class for all of the views. This class is named
    /// in the web.config spark/pages/@pageTypeName setting.
    /// 
    /// Properties and methods can be added for use in spark templates.
    /// </summary>
    public abstract class BaseView : AbstractSparkView
    {
        protected BaseView()
        {
            ViewData = new ViewDataDictionary();
        }

        /// <summary>
        /// Context is assigned by BaseHandler.CreateView
        /// </summary>
        public HttpContext Context { get; set; }

        /// <summary>
        /// The generated code will use ViewData.Eval("propertyname") if 
        /// the template is using the viewdata element
        /// </summary>
        public ViewDataDictionary ViewData { get; set; }

        /// <summary>
        /// Provides a normalized application path
        /// </summary>
        public string SiteRoot
        {
            get
            {
                string[] parts = Context.Request.ApplicationPath.Split(new[] {'/', '\\'},
                                                                       StringSplitOptions.RemoveEmptyEntries);
                return string.Concat(parts.Select(part => "/" + part).ToArray());
            }
        }

        /// <summary>
        /// The generated code will use the SiteResource method when
        /// an html attribute for a url starts with ~
        /// </summary>
        public string SiteResource(string path)
        {
            if (path.StartsWith("~/"))
                return SiteRoot + path.Substring(1);
            return path;
        }

        /// <summary>
        /// ${H(blah)} is a convenience to htmlencode the value of blah.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string H(object text)
        {
            return Context.Server.HtmlEncode(Convert.ToString(text));
        }

        #region Nested type: ViewDataDictionary

        public class ViewDataDictionary : Dictionary<string, object>
        {
            public object Eval(string key)
            {
                return this[key];
            }
        }

        #endregion
    }
}