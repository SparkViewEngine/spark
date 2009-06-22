using System;
using System.Collections.Generic;
using System.Web.Mvc;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace IntegrationTestingViews.Tests
{
    /// <summary>
    /// These tests simply test the rendering of the view as you would expect it. 
    /// </summary>
    [TestFixture]
    public class ViewTests
    {

        [Test]
        public void RenderTemplateWithoutPartial()
        {
            var viewData = new ViewDataDictionary { { "Message", "Holy Cow" } };
            var result = SparkTestHelpers.RenderTemplate(@"Home\Index.spark", viewData);
            Assert.That(result.Contains("The message is: Holy Cow"), Is.True, String.Format("Uh oh ... Could not find the holy cow on the index page. Result was: '{0}'", result));
        }

        [Test]
        public void RenderTemplateWithPartial()
        {
            var result = SparkTestHelpers.RenderTemplate(@"Home\WithPartial.spark");
            Assert.That(result.Contains("Before Foo. | This is foo. | After Foo"), Is.True, String.Format("Uh oh ... Could not find the foo in the middle. Result was: '{0}'", result));
        }

        [Test]
        public void RenderViewWithoutPartial()
        {
            var viewData = new ViewDataDictionary { {"Message", "Holy Cow"} };
            var result = SparkTestHelpers.RenderPartialView(@"Home", "Index", viewData);
            Assert.That(result.Contains("The message is: Holy Cow"), Is.True, String.Format("Uh oh ... Could not find the holy cow on the index page. Result was: '{0}'", result));
        }

        [Test]
        public void RenderViewWithPartial()
        {
            var result = SparkTestHelpers.RenderPartialView("Home", "WithPartial");
            Assert.That(result.Contains("Before Foo. | This is foo. | After Foo"), Is.True, String.Format("Uh oh ... Could not find the foo in the middle. Result was: '{0}'", result));
        }
    }
}