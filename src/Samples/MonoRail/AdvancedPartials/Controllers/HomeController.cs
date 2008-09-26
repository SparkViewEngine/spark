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
using AdvancedPartials.Models;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;

namespace AdvancedPartials.Controllers
{
    [Layout("default")]
    public class HomeController : SmartDispatcherController
    {
        public void Index()
        {
        }

        public void About()
        {
        }

        public void AddingAtPlaceholders()
        {
            
        }

        public void StyleGuide()
        {
            
        }

        public void Boxes()
        {
            
        }

        public void PagingAndRepeater(int id)
        {
            var repos = new BirdRepository();
            var birds = repos.GetBirds();

            var items = 
                PaginationHelper.CreatePagination(
                    birds, // list
                    10, // number of items per page
                    id
                );
            
            PropertyBag["items"] = items;
        }
    }
}
