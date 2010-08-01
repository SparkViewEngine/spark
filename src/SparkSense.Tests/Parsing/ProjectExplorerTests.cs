using System;
using System.Runtime.InteropServices;
using EnvDTE;
using NUnit.Framework;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Syntax;
using SparkSense.Parsing;

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
            new ProjectExplorer(null);
        }

        [Test] //TODO Rob G , Ignore("This test fails if run without an instance of VS running. It passes if it can attach to the DTE. Need to find a better way of testing this")]
        public void ShouldBuildAMapOfAllViewsInTheCurrentProject()
        {
            Console.WriteLine("This test fails if run without an instance of VS running. It passes if it can attach to the DTE. Need to find a better way of testing this");
            var projectEnvironment = (DTE)Marshal.GetActiveObject("VisualStudio.DTE.10.0");

            var projectExplorer = new ProjectExplorer(projectEnvironment);

            Assert.That(projectExplorer.HasView("Shared\\_SharedPartial.spark"));
            Assert.That(projectExplorer.HasView("Shared\\Application.spark"));
        }
    }
}
