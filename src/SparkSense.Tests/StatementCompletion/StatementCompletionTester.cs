using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;
using Spark;
using Spark.Parser;
using Spark.Parser.Syntax;

namespace SparkSense.Tests
{
    [TestFixture]
    public class StatementCompletionTester
    {
        private const string ROOT_VIEW_PATH = "SparkSense.Tests.Views";
        private DefaultSyntaxProvider _syntaxProvider;
        private ViewLoader _viewLoader;
        public void SparkTagCompletionTests()
        {

        }

        [SetUp]
        public void Setup()
        {
            _syntaxProvider = new DefaultSyntaxProvider(new ParserSettings());
            _viewLoader = new ViewLoader { ViewFolder = new FileSystemViewFolder("SparkSense.Tests.Views"), SyntaxProvider = _syntaxProvider };
        }

        [Test]
        public void ShouldRecogniseOtherViewsInTheProject()
        {
            var engine = new SparkViewEngine();
            engine.ViewFolder = new FileSystemViewFolder("SparkSense.Tests.Views");

            var shared = engine.ViewFolder.ListViews("Home");

            shared.ToList().ForEach(s => Console.WriteLine(s));

        }

        [Test]
        public void ShouldRecogniseContentAreasInTheProject()
        {
        }

        [Test]
        public void ShouldRecogniseVariablesDeclaredInTheSameFile()
        {
            var viewFolder = new InMemoryViewFolder{
                {"test\\ContainsTwoVars.spark", "<var x=\"5\" y=\"3\" />"}
            };

            var analyzer = new SparkViewAnalyzer(viewFolder, "test\\ContainsTwoVars.spark");
            IList<string> vars = analyzer.GetLocalVariables();
            Assert.AreEqual(2, vars.Count);
            Assert.AreEqual("x", vars[0]);
            Assert.AreEqual("y", vars[1]);
        }

    }
}
