using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.FileSystem;
using SparkSense.Parsing;
using Rhino.Mocks;

namespace SparkSense.Tests.Parsing
{
    [TestFixture]
    public class ViewExplorerTests
    {
        private IProjectExplorer _mockProjectExplorer;

        [SetUp]
        public void Setup()
        {
            _mockProjectExplorer = MockRepository.GenerateMock<IProjectExplorer>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProjectExplorer.VerifyAllExpectations();
            _mockProjectExplorer = null;
        }

        [Test]
        public void ShouldRecogniseVariablesDeclaredInTheSameFile()
        {

            var filePath = "test\\TwoVars.spark";
            var fileContent = "<var theNumberFive=\"5\" theNumberThree=\"3\" />";
            var viewFolder = new InMemoryViewFolder { { filePath, fileContent } };

            _mockProjectExplorer.Expect(x => x.GetViewFolder()).Return(viewFolder);
            _mockProjectExplorer.Expect(x => x.GetCurrentView()).Return(filePath);

            var viewExplorer = new ViewExplorer(_mockProjectExplorer);
            IList<string> vars = viewExplorer.GetLocalVariables();

            Assert.That(vars.Count, Is.EqualTo(2));
            Assert.That(vars[0], Is.EqualTo("theNumberFive"));
            Assert.That(vars[1], Is.EqualTo("theNumberThree"));
        }

        [Test]
        public void ShouldRecogniseMacrosDeclaredInTheSameFile()
        {

            var filePath = "test\\TwoMacros.spark";
            var fileContent = "<div><macro name=\"Macro1\">one</macro></div><div><macro name=\"Macro2\">two</macro></div>";
            var viewFolder = new InMemoryViewFolder { { filePath, fileContent } };

            _mockProjectExplorer.Expect(x => x.GetViewFolder()).Return(viewFolder);
            _mockProjectExplorer.Expect(x => x.GetCurrentView()).Return(filePath);

            var viewExplorer = new ViewExplorer(_mockProjectExplorer);
            IList<string> macros = viewExplorer.GetLocalMacros();

            Assert.That(macros.Count, Is.EqualTo(2));
            Assert.That(macros[0], Is.EqualTo("Macro1"));
            Assert.That(macros[1], Is.EqualTo("Macro2"));
        }

        [Test]
        public void ShouldRecogniseMacroParameters()
        {
            var filePath = "test\\TwoMacrosSecondWithParam.spark";
            var fileContent = "<div><macro name=\"Macro1\">one</macro></div><div><macro name=\"Macro2\" param1=\"string\">two</macro></div>";
            var viewFolder = new InMemoryViewFolder { { filePath, fileContent } };

            _mockProjectExplorer.Expect(x => x.GetViewFolder()).Return(viewFolder);
            _mockProjectExplorer.Expect(x => x.GetCurrentView()).Return(filePath);

            var viewExplorer = new ViewExplorer(_mockProjectExplorer);
            IList<string> macroParams = viewExplorer.GetMacroParameters("Macro1");
            Assert.That(macroParams.Count, Is.EqualTo(0));

            macroParams = viewExplorer.GetMacroParameters("Macro2");
            Assert.That(macroParams.Count, Is.EqualTo(1));
            Assert.That(macroParams[0], Is.EqualTo("param1"));
        }

        [Test]
        public void ShouldReturnNameOfPartialsFound()
        {
            var viewFolder = new InMemoryViewFolder
            {
                    {"Shared\\Application.spark","<html><body><use content=\"main\" /></body></html>"},
                    {"Shared\\_PartialMustBeFound.spark","This partial should always be found"},
                    {"Home\\index.spark","Home Page"},
                    {"Home\\_HomePartial.spark","This Partial should only be found from Home"},
                    {"Other\\index.spark","Home Page"},
                    {"Other\\_OtherPartial.spark","This Partial should only be found from Other"},
            };

            _mockProjectExplorer.Expect(x => x.GetViewFolder()).Return(viewFolder);
            _mockProjectExplorer.Expect(x => x.GetCurrentView()).Return("Home\\index.spark");

            var homeExplorer = new ViewExplorer(_mockProjectExplorer);
            var homePartials = homeExplorer.GetRelatedPartials();

            _mockProjectExplorer.Expect(x => x.GetViewFolder()).Return(viewFolder);
            _mockProjectExplorer.Expect(x => x.GetCurrentView()).Return("Other\\index.spark");

            var otherExplorer = new ViewExplorer(_mockProjectExplorer);
            var otherPartials = otherExplorer.GetRelatedPartials();

            Assert.That(homePartials.Count, Is.EqualTo(2));
            Assert.That(homePartials[0], Is.EqualTo("HomePartial"));
            Assert.That(homePartials[1], Is.EqualTo("PartialMustBeFound"));

            Assert.That(otherPartials.Count, Is.EqualTo(2));
            Assert.That(otherPartials[0], Is.EqualTo("OtherPartial"));
            Assert.That(otherPartials[1], Is.EqualTo("PartialMustBeFound"));
        }

        [Test]
        public void ShouldReturnNameOfPossibleMasterLayoutsFound()
        {
            var viewFolder = new InMemoryViewFolder
            {
                    {"Shared\\Home.spark","<html><body><use content=\"home\" /></body></html>"},
                    {"Shared\\Application.spark","<html><body><use content=\"main\" /></body></html>"},
                    {"Shared\\_PartialMustNotBeMaster.spark","This partial should not be identified as a master layout"},
                    {"Layouts\\Other.spark","<html><body><use content=\"other\" /></body></html>"},
                    {"Home\\index.spark","Home Page"},
            };

            _mockProjectExplorer.Expect(x => x.GetViewFolder()).Return(viewFolder);
            _mockProjectExplorer.Expect(x => x.GetCurrentView()).Return("Home\\index.spark");

            var viewExplorer = new ViewExplorer(_mockProjectExplorer);
            var possibleMasters = viewExplorer.GetPossibleMasterLayouts().ToList();

            Assert.That(possibleMasters.Count, Is.EqualTo(3));
            Assert.Contains("Application", possibleMasters);
            Assert.Contains("Home", possibleMasters);
            Assert.Contains("Other", possibleMasters);
        }
    }
}
