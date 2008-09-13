using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;

namespace Castle.MonoRail.Views.Spark.Tests.Helpers
{
    public class TestingHelper : AbstractHelper
    {
        public string Foo()
        {
            return "Hello";
        }
    }

    [Helper(typeof(TestingHelper), "bar")]
    public class HomeController : SmartDispatcherController
    {
        public void Index()
        {
            
        }
    }
}
