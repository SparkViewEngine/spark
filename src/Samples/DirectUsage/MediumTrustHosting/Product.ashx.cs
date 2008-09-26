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
using MediumTrustHosting.Models;

namespace MediumTrustHosting
{
    public class Product : BaseHandler
    {
        public override void Process()
        {
            int id = Convert.ToInt32(Context.Request.Params["id"]);

            var repos = new ProductRepository();

            BaseView view = CreateView("product.spark", "master.spark");
            view.ViewData["product"] = repos.Fetch(id);
            view.RenderView(Context.Response.Output);
        }
    }
}