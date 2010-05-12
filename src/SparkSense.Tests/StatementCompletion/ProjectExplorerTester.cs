using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Spark.FileSystem;
using Spark;
using Spark.Parser;
using Spark.Parser.Syntax;
using NUnit.Framework.SyntaxHelpers;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using System.Runtime.InteropServices;

namespace SparkSense.Tests
{
    [TestFixture]
    public class ProjectExplorerTester
    {
        private const string IRRELEVANT_FILE_CONTENT = "Irrelevant content";
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
            new SparkProjectExplorer(null);
        }

        [Test]
        public void ShouldBuildAMapOfAllViewsInTheSolution()
        {
            var projectEnvironment = (DTE)Marshal.GetActiveObject("VisualStudio.DTE.10.0");

            var projectExplorer = new SparkProjectExplorer(projectEnvironment);
            var viewMap = projectExplorer.ViewMap;

            viewMap.ForEach(s => Console.WriteLine(s));

            Assert.Contains("Shared\\Application.spark", viewMap);
        }
    }
}
