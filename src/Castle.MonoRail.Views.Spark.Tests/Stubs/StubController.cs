using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;

namespace Castle.MonoRail.Views.Spark.Tests.Stubs
{
    [Layout("default")]
    public class StubController : SmartDispatcherController
    {
        public void Index()
        {
        }
        
        public void List()
        {
        }

        [Layout("ajax")]
        public void _Widget()
        {
            
        }
    }
}
