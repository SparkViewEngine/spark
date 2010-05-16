using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.FileSystem;
using SparkSense.Parsing;

namespace SparkSense.Tests.Parsing
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

            var viewExplorer = new ViewExplorer(viewFolder, filePath);
            IList<string> vars = viewExplorer.GetLocalVariables();

            Assert.That(vars.Count, Is.EqualTo(2));
            Assert.That(vars[0], Is.EqualTo("theNumberFive"));
            Assert.That(vars[1], Is.EqualTo("theNumberThree"));
        }

        [Test]
        public void ShouldReturnNameOfPartialsFound()
        {
            var viewFolder = new InMemoryViewFolder
            {
                    {"Shared\\Application.spark","<html><body><use content=\"main\"></body></html>"},
                    {"Shared\\_PartialMustBeFound.spark","This partial should always be found"},
                    {"Home\\index.spark","Home Page"},
                    {"Home\\_HomePartial.spark","This Partial should only be found from Home"},
                    {"Other\\index.spark","Home Page"},
                    {"Other\\_OtherPartial.spark","This Partial should only be found from Other"},
            };

            var homeExplorer = new ViewExplorer(viewFolder, "Home\\index.spark");
            var homePartials = homeExplorer.GetRelatedPartials();

            var otherExplorer = new ViewExplorer(viewFolder, "Other\\index.spark");
            var otherPartials = otherExplorer.GetRelatedPartials();

            Assert.That(homePartials.Count, Is.EqualTo(2));
            Assert.That(homePartials[0], Is.EqualTo("HomePartial"));
            Assert.That(homePartials[1], Is.EqualTo("PartialMustBeFound"));

            Assert.That(otherPartials.Count, Is.EqualTo(2));
            Assert.That(otherPartials[0], Is.EqualTo("OtherPartial"));
            Assert.That(otherPartials[1], Is.EqualTo("PartialMustBeFound"));
        }
    }
}
