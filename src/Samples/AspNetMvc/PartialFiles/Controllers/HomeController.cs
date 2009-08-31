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

namespace PartialFiles.Controllers
{
    /// <summary>
    /// Shows the use of index.spark + defaultlayout.spark, 
    /// and alternate.aspx + defaultlayout.master, both of which
    /// render a mix of spark, aspx, and ascx partial files.
    /// </summary>
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View("Index", "DefaultLayout");
        }

        public ActionResult Alternate()
        {
            return View("Alternate", "DefaultLayout");
        }

        public ActionResult ShowStatus(string code)
        {
            return PartialView("_Status", new { code });
        }
    }
}