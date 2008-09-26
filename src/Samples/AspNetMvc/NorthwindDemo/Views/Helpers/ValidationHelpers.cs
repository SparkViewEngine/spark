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
using System.Web.Mvc;

namespace NorthwindDemo.Views.Helpers
{
    public static class ValidationHelpers
    {
        public static string Label(this HtmlHelper html, string text, string name)
        {
            string css = string.Empty;

            if (html.ViewContext.ViewData.ContainsKey("Error:" + name))
            {
                css = " class=\"error\"";
            }

            string format = "<label for=\"{0}\"{2}>{1}</label>";
            return string.Format(format, name, text, css);
        }
    }
}