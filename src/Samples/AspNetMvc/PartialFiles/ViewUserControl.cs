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
using System.Web.UI;

namespace PartialFiles
{
    /// <summary>
    /// Default ViewUserControl base class for aspc files in this project.
    /// Set via system.web/pages/@userControlBaseType
    /// </summary>
    public class ViewUserControl : System.Web.Mvc.ViewUserControl
    {
        /// <summary>
        /// Must substitute the ViewContext TextWriter, because 
        /// the native HttpContext.Current TextWriter will be used by default
        /// </summary>
        public override void RenderControl(HtmlTextWriter writer)
        {
            if (ViewContext != null)
                writer.InnerWriter = ViewContext.HttpContext.Response.Output;

            base.RenderControl(writer);
        }
    }
}