// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
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
using System.Collections.Generic;
using System.IO;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Test;
using NUnit.Framework;
using Rhino.Mocks;

namespace Castle.MonoRail.Views.Spark.Tests.ViewComponents
{
    [TestFixture]
    public class ViewComponentExtensionTests : BaseViewComponentTests
    {
        

        [Test]
        public void DiggPaginationComponent()
        {
            var writer = new StringWriter();
            IList<string> dataSource = new List<string>();
            for(int i = 100; i != 200; i++)
                dataSource.Add(i.ToString());

            controllerContext.PropertyBag["items"] = PaginationHelper.CreatePagination(dataSource, 10, 3);

            factory.Process(string.Format("Home{0}DiggPaginationComponent", Path.DirectorySeparatorChar), writer,
                            engineContext, controller, controllerContext);

            ContainsInOrder(writer.ToString(),
                            "<li>120</li>", "<li>129</li>");
        }

        static void ContainsInOrder(string content, params string[] values)
        {
            int index = 0;
            foreach (string value in values)
            {
                int nextIndex = content.IndexOf(value, index);
                Assert.GreaterOrEqual(nextIndex, 0, string.Format("Looking for {0}", value));
                index = nextIndex + value.Length;
            }
        }


		[ViewComponentDetails("Simple")]
		public class SimpleViewComponent : ViewComponent
		{
			[ViewComponentParam]
			public string Caption { get; set; }

			public override void Render()
			{
				RenderText("<p>" + Caption + "</p>");
			}
		}

		[Test]
		public void ComponentsSupportEachAttribute()
		{
			viewComponentFactory.Registry.AddViewComponent("Simple", typeof(SimpleViewComponent));

			var writer = new StringWriter();
		    factory.Process(string.Format("Home{0}ComponentsSupportEachAttribute", Path.DirectorySeparatorChar), writer,
		                    engineContext, controller, controllerContext);

			ContainsInOrder(writer.ToString(),
							"<p>alpha</p>",
							"<p>beta</p>",
							"<p>gamma</p>");
		}

    	[Test]
		public void ComponentsSupportIfAttribute()
		{
			viewComponentFactory.Registry.AddViewComponent("Simple", typeof(SimpleViewComponent));

			var writer = new StringWriter();
    	    factory.Process(string.Format("Home{0}ComponentsSupportIfAttribute", Path.DirectorySeparatorChar), writer,
    	                    engineContext, controller, controllerContext);

			ContainsInOrder(writer.ToString(),
							"<p>foo1</p>",
							"<p>alpha</p>",
							"<p>gamma</p>");
			Assert.IsFalse(writer.ToString().Contains("foo2"));
			Assert.IsFalse(writer.ToString().Contains("beta"));
		}

		[Test]
		public void ComponentsSupportOnceAttribute()
		{
			viewComponentFactory.Registry.AddViewComponent("Simple", typeof(SimpleViewComponent));

			var writer = new StringWriter();
		    factory.Process(string.Format("Home{0}ComponentsSupportOnceAttribute", Path.DirectorySeparatorChar), writer,
		                    engineContext, controller, controllerContext);

			ContainsInOrder(writer.ToString(),
							"<p>foo1</p>",
							"<p>foo2</p>");
			Assert.IsFalse(writer.ToString().Contains("foo3"));
		}
	}
}