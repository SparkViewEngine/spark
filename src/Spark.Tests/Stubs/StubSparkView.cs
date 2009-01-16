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
using System.Web;

namespace Spark.Tests.Stubs
{
    public abstract class StubSparkView : AbstractSparkView
    {
        protected StubSparkView()
        {
            ViewData = new StubViewData();
        }

        public StubViewData ViewData { get; set; }

        public string SiteRoot
        {
            get { return "/TestApp"; }
        }

        public string SiteResource(string path)
        {
            return SiteRoot + path;
        }

        public override bool TryGetViewData(string name, out object value)
        {
            return ViewData.TryGetValue(name, out value);
        }

        public string H(object content)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(content));
        }

        public object Eval(string expression)
        {
            return ViewData.Eval(expression);
        }
    }

    public abstract class StubSparkView<TModel> : StubSparkView
    {
        public new StubViewData<TModel> ViewData
        {
            get { return (StubViewData<TModel>)base.ViewData; }
            set { base.ViewData = value; }
        }
    }
}
