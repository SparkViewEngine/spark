using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MvcContrib.SparkViewEngine;
using MvcContrib.ViewFactories;
using NUnit.Framework;
using Spark.FileSystem;

namespace MvcContrib.UnitTests.SparkViewEngine
{
	[TestFixture]
	public class ViewSourceLoaderWrapperTester
	{
		[Test]
		public void ResultsAreTheSame()
		{
			IViewSourceLoader loader = new FileSystemViewSourceLoader("AspNetMvc.Tests.Views");
			IViewFolder wrapper = new ViewSourceLoaderWrapper(loader);

            Assert.AreEqual(loader.HasView("Home\\foreach.spark"), wrapper.HasView("Home\\foreach.spark"));
            Assert.AreEqual(loader.HasView("Home\\nosuchfile.spark"), wrapper.HasView("Home\\nosuchfile.spark"));

			var loaderViews = loader.ListViews("Shared");
			var wrapperViews = wrapper.ListViews("Shared");
			Assert.AreEqual(loaderViews.Count(), wrapperViews.Count);

			foreach(string viewName in loaderViews)
			{
				Assert.That(wrapperViews.Contains(viewName));
			}
            
            var loaderView = loader.GetViewSource("Home\\foreach.spark");
            var wrapperView = wrapper.GetViewSource("Home\\foreach.spark");

			Assert.AreEqual(loaderView.LastModified, wrapperView.LastModified);

			var loaderReader = new StreamReader(loaderView.OpenViewStream());
			var wrapperReader = new StreamReader(wrapperView.OpenViewStream());
			Assert.AreEqual(loaderReader.ReadToEnd(), wrapperReader.ReadToEnd());
		}
	}

}
