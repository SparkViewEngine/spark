using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Routing;
using System.Web.Mvc;
using Moq;
using System.Web;

namespace UnitTests
{
    [TestClass]
    public class RoutingTest
    {
[TestMethod]
public void GetVirtualPathCanFillInSeparatedParametersWithDefaultValues()
{
    var routes = new RouteCollection();
    routes.MapRoute("some-route", "{controller}/{language}-{locale}", new {language="en", locale="US"});

    var request = new Mock<HttpRequestBase>();
    request.Expect(req => req.AppRelativeCurrentExecutionFilePath).Returns("~/");
    request.Expect(req => req.ApplicationPath).Returns("");

    var httpContext = new Mock<HttpContextBase>();
    httpContext.Expect(http => http.Request).Returns(request.Object);

    var context = new RequestContext(httpContext.Object, new RouteData());

    VirtualPathData vpd = routes.GetVirtualPath(context, new RouteValueDictionary(new { controller = "Orders" }));
    Assert.IsNotNull(vpd, "Expected this to match our one route");
    Assert.AreEqual("/Orders/en-US", vpd.VirtualPath, "Expected the route to fill in the parameters using the defaults");
}
    }
}
