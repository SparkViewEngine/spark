using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark;
using Spark.FileSystem;
using Spark.Web.Mvc;

namespace IntegrationAndViewTests
{
    /// <summary>
    /// These tests simply test the rendering of the view as you would expect it. 
    /// </summary>
    [TestFixture]
    public class ViewTests
    {
        [Test]
        public void RenderViewWithoutPartial()
        {
            var viewData = new ViewDataDictionary { {"Message", "Holy Cow"} };
            var result = SparkTestHelpers.RenderContents(@"\Views\Home", "Index.spark", viewData);
            Assert.That(result.Contains("The message is: Holy Cow"), Is.True, String.Format("Uh oh ... Could not find the holy cow on the index page. Result was: '{0}'", result));
        }

        [Test]
        public void RenderViewWithPartial()
        {
            var result = SparkTestHelpers.RenderContents(@"Views\Home", "WithPartial.spark");
            Assert.That(result.Contains("Before Foo. | This is foo. | After Foo"), Is.True, String.Format("Uh oh ... Could not find the foo in the middle. Result was: '{0}'", result));
        }
    }

    public class SparkTestHelpers
    {
        public static string RenderContents(string viewFolder, string viewName)
        {
            return RenderContents(viewFolder, viewName, new ViewDataDictionary());
        }

        public static string RenderContents(string viewFolder, string viewName, ViewDataDictionary viewData)
        {
            // Set up your spark engine goodness. 
            var settings = new SparkSettings().SetPageBaseType(typeof(SparkView));
            settings.Debug = true; 

            var templateRoot = new FileSystemViewFolder(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\SparkWebApp\Views");
            var mainTemplate = new FileSystemViewFolder(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\SparkWebApp\" + viewFolder);

            var templates = new CombinedViewFolder(templateRoot, mainTemplate); 
          
            var engine = new SparkViewEngine(settings)
                             {
                                 ViewFolder = templates
                             };
            
            // "Describe" the view (the template, it is a template after all), and its details.
            var descriptor = new SparkViewDescriptor().AddTemplate(viewName);

            // Create a spark view engine instance
            var view = (SparkView)engine.CreateInstance(descriptor);

            // Merge the view data. 
            viewData.Keys.ToList().ForEach(x => view.ViewData[x] = viewData[x]);

            // Render the view to a text writer. 
            var writer = new StringWriter();
            view.RenderView(writer);
            return writer.ToString();

            
        }
    }
}