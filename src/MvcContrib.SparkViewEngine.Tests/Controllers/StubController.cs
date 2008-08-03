using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace MvcContrib.SparkViewEngine.Tests.Controllers
{
    public class StubController : IController
    {
        public void Execute(ControllerContext controllerContext)
        {
            throw new System.NotImplementedException();
        }
    }
}
