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
using WindsorInversionOfControl.Models;

namespace WindsorInversionOfControl.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISampleRepository _repository;

        public HomeController(ISampleRepository repository)
        {
            _repository = repository;

            //default intro message
            IntroMessage = "Hello World";
        }

        public string IntroMessage { get; set; }

        public ActionResult Index()
        {
            ViewData["Intro"] = IntroMessage;
            ViewData["Products"] = _repository.GetProducts();

            return View("Index");
        }
    }
}