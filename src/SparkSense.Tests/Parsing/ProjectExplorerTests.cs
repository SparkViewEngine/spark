using System;
using System.Runtime.InteropServices;
using EnvDTE;
using NUnit.Framework;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Syntax;

namespace SparkSense.Tests.Parsing
{
    [TestFixture]
    public class ProjectExplorerTests
    {
        private const string ROOT_VIEW_PATH = "SparkSense.Tests.Views";
        private DefaultSyntaxProvider _syntaxProvider;
        private ViewLoader _viewLoader;

        [SetUp]
        public void Setup()
        {
            _syntaxProvider = new DefaultSyntaxProvider(new ParserSettings());
            _viewLoader = new ViewLoader { ViewFolder = new FileSystemViewFolder(ROOT_VIEW_PATH), SyntaxProvider = _syntaxProvider };
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfTheProjectEnvironmentIsNull()
        {
            new SparkSense.Parsing.ProjectExplorer(null);
        }

        [Test] //, Ignore("This test fails if run without an instance of VS running. It passes if it can attach to the DTE. Need to find a better way of testing this")]
        public void ShouldBuildAMapOfAllViewsInTheSolution()
        {
            Console.WriteLine("This test fails if run without an instance of VS running. It passes if it can attach to the DTE. Need to find a better way of testing this");
            var projectEnvironment = (DTE)Marshal.GetActiveObject("VisualStudio.DTE.10.0");

            var projectExplorer = new SparkSense.Parsing.ProjectExplorer(projectEnvironment);
            var viewMap = projectExplorer.ViewMap;

            Assert.Contains("Shared\\_SharedPartial.spark", viewMap);
            Assert.Contains("Shared\\Application.spark", viewMap);
        }
    }
}
