using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using MvcIntegrationTestFramework.Browsing;
using MvcIntegrationTestFramework.Hosting;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace IntegrationTestingViews.Tests
{
    /// <summary>
    /// These tests are using the framework provided by Steve Sanderson: 
    /// http://blog.codeville.net/2009/06/11/integration-testing-your-aspnet-mvc-application/
    /// </summary>
    [TestFixture]
    public class EndToEndIntegrationTests
    {
        private static readonly string _mvcAppPath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\IntegrationTestingViews");
        private readonly AppHost _appHost = new AppHost(_mvcAppPath);

        [Test]
        public void ShouldRenderTheSparkViewAndReadTheContents()
        {
            _appHost.SimulateBrowsingSession(browsingSession =>
                                                 {
                                                     // Request the root URL
                                                     RequestResult result = browsingSession.ProcessRequest("");

                                                     // Can make assertions about the ActionResult...
                                                     var viewResult = (ViewResult)result.ActionExecutedContext.Result;
                                                     Assert.AreEqual("Index", viewResult.ViewName);
                                                     Assert.AreEqual("foo", viewResult.ViewData["Message"]);

                                                     // ... or can make assertions about the rendered HTML
                                                     Assert.That(result.ResponseText.Contains(@"<title>Integration Tests</title>"), Is.True);
                                                     Assert.That(result.ResponseText.Contains("The message is: foo"), Is.True); 
                                                 });
        }

        [Test]
        public void ShouldBeAbleToRenderAPartialAndViewItsContentsInTheConsumer()
        {
            _appHost.SimulateBrowsingSession(browsingSession =>
                                                 {
                                                     // Request the root URL
                                                     RequestResult result = browsingSession.ProcessRequest("home/withpartial");

                                                     // Can make assertions about the ActionResult...
                                                     var viewResult = (ViewResult)result.ActionExecutedContext.Result;
                                                     Assert.AreEqual("withpartial", viewResult.ViewName);

                                                     // ... or can make assertions about the rendered HTML
                                                     Assert.That(result.ResponseText.Contains(@"Before Foo. | This is foo. | After Foo"), Is.True);
                                                 });
        }
    }
}