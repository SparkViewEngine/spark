using System;
using System.Collections.Generic;
using NUnit.Framework;
using Spark.FileSystem;
using NUnit.Framework.SyntaxHelpers;

namespace SparkSense.Tests
{
    [TestFixture]
    public class ViewExplorerTester
    {
        [Test]
        public void ShouldRecogniseVariablesDeclaredInTheSameFile()
        {
            var filePath = "test\\TwoVars.spark";
            var fileContent = "<var theNumberFive=\"5\" theNumberThree=\"3\" />";
            var viewFolder = new InMemoryViewFolder { { filePath, fileContent } };

            var viewExplorer = new SparkViewExplorer(viewFolder, filePath);
            IList<string> vars = viewExplorer.GetLocalVariables();

            Assert.That(vars.Count, Is.EqualTo(2));
            Assert.That(vars[0], Is.EqualTo("theNumberFive"));
            Assert.That(vars[1], Is.EqualTo("theNumberThree"));
        }
    }
}
