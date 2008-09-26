using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Spark.Web.Mvc.Tests.Controllers
{
    [Precompile(Exclude = "Show*", Layout = "Default")]
    [Precompile(Include = "_Foo _Bar", Layout = "Ajax")]
    [Precompile(Include = "Show*", Layout = "Showing")]
    public class ComplexPrecompileController : Controller
    {
    }
}
