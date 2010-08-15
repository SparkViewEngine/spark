using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.FileSystem;
using SparkSense.Parsing;
using Rhino.Mocks;
using System.IO;

namespace SparkSense.Tests.Parsing
{
    [TestFixture]
    public class ViewExplorerTests
    {
        private const string ROOT_VIEW_PATH = "SparkSense.Tests.Views";
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

            var viewExplorer = new ViewExplorer(_mockProjectExplorer, filePath);
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

            var viewExplorer = new ViewExplorer(_mockProjectExplorer, filePath);
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

            var viewExplorer = new ViewExplorer(_mockProjectExplorer, filePath);
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

            var homeExplorer = new ViewExplorer(_mockProjectExplorer, "Home\\index.spark");
            var homePartials = homeExplorer.GetRelatedPartials();

            _mockProjectExplorer.Expect(x => x.GetViewFolder()).Return(viewFolder);

            var otherExplorer = new ViewExplorer(_mockProjectExplorer, "Other\\index.spark");
            var otherPartials = otherExplorer.GetRelatedPartials();

            Assert.That(homePartials.Count, Is.EqualTo(2));
            Assert.That(homePartials[0], Is.EqualTo("HomePartial"));
            Assert.That(homePartials[1], Is.EqualTo("PartialMustBeFound"));

            Assert.That(otherPartials.Count, Is.EqualTo(2));
            Assert.That(otherPartials[0], Is.EqualTo("OtherPartial"));
            Assert.That(otherPartials[1], Is.EqualTo("PartialMustBeFound"));
        }

        [Test]
        public void ShouldReturnDefaultParametersOfPartial()
        {
            var viewFolder = new InMemoryViewFolder
            {
                    {"Shared\\_SharedPartial.spark","<default sx='5' sy='10' /> This partial is shared"},
                    {"Home\\_HomePartial.spark","<default hx='8' hy='16' /> This Partial should only be found from Home"},
                    {"Home\\index.spark","Home Page <SharedPartial /><HomePartial />"},
            };

            _mockProjectExplorer.Expect(x => x.GetViewFolder()).Return(viewFolder);

            var homeExplorer = new ViewExplorer(_mockProjectExplorer, "Home\\index.spark");
            var homeParameters = homeExplorer.GetPossiblePartialDefaults("HomePartial");
            var sharedParameters = homeExplorer.GetPossiblePartialDefaults("SharedPartial");

            Assert.That(homeParameters.Count, Is.EqualTo(2));
            Assert.That(homeParameters[0], Is.EqualTo("hx"));
            Assert.That(homeParameters[1], Is.EqualTo("hy"));

            Assert.That(sharedParameters.Count, Is.EqualTo(2));
            Assert.That(sharedParameters[0], Is.EqualTo("sx"));
            Assert.That(sharedParameters[1], Is.EqualTo("sy"));
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

            var viewExplorer = new ViewExplorer(_mockProjectExplorer, "Home\\index.spark");
            var possibleMasters = viewExplorer.GetPossibleMasterLayouts().ToList();

            Assert.That(possibleMasters.Count, Is.EqualTo(3));
            Assert.Contains("Application", possibleMasters);
            Assert.Contains("Home", possibleMasters);
            Assert.Contains("Other", possibleMasters);
        }

        [Test]
        public void ShouldBeAbleToEvictViewChunksWhenChangedInMemory()
        {
            string key = "Shared\\test.spark";
            string content = "<var x='5'/>";
            var viewFolder = new CachingViewFolder(Path.GetFullPath(ROOT_VIEW_PATH));

            _mockProjectExplorer.Expect(x => x.GetViewFolder()).Return(viewFolder);

            var viewExplorer = new ViewExplorer(_mockProjectExplorer, key);
            var localVars = viewExplorer.GetLocalVariables();
            Assert.That(localVars.Count, Is.EqualTo(5));

            content += "<var y='25' />";
            _mockProjectExplorer.Expect(x => x.SetViewContent(key, content))
                .WhenCalled(x => viewFolder.SetViewSource(key, content));

            viewExplorer.InvalidateView(content);
            localVars = viewExplorer.GetLocalVariables();
            Assert.That(localVars.Count, Is.EqualTo(2));
        }
    }
}
