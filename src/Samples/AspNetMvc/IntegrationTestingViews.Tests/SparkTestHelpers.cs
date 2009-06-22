using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Spark;
using Spark.FileSystem;
using Spark.Web.Mvc;

namespace IntegrationTestingViews.Tests
{
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

            var templateRoot = new FileSystemViewFolder(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\IntegrationTestingViews\Views");
            var mainTemplate = new FileSystemViewFolder(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\IntegrationTestingViews\" + viewFolder);

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