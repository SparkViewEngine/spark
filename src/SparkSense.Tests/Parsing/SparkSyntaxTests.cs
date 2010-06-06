using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using SparkSense.Parsing;
using Spark.Parser.Markup;

namespace SparkSense.Tests.Parsing
{
    [TestFixture]
    public class SparkSyntaxTests
    {
        [Test]
        public void ShouldTriggerTagCompletionForOpeningTags()
        {
            var sparkSyntax = new SparkSyntax(null);

            SparkSyntaxTypes syntaxType;
            sparkSyntax.IsSparkSyntax('<', out syntaxType);

            Assert.That(syntaxType, Is.EqualTo(SparkSyntaxTypes.Element)); 
        }

        [Test]
        public void ShouldTriggerAttributeCompletionWhenInsideATag()
        {
            var mockExplorer = MockRepository.GenerateMock<ITextExplorer>();

            Node mockNode = new ElementNode("someTag", null, true);
            mockExplorer.Expect(x => x.GetStartPosition()).Return(17);
            mockExplorer.Expect(x => x.GetNodeAtPosition(17)).Return(mockNode);
            
            var sparkSyntax = new SparkSyntax(mockExplorer);
            SparkSyntaxTypes syntaxType;
            sparkSyntax.IsSparkSyntax(' ', out syntaxType);

            Assert.That(syntaxType, Is.EqualTo(SparkSyntaxTypes.Attribute));
        }

        [Test]
        public void ShouldNotTriggerAttributeCompletionWhenOutsideATag()
        {
            var mockExplorer = MockRepository.GenerateMock<ITextExplorer>();

            Node mockNode = new TextNode("some text");
            mockExplorer.Expect(x => x.GetStartPosition()).Return(17);
            mockExplorer.Expect(x => x.GetNodeAtPosition(17)).Return(mockNode);
            
            var sparkSyntax = new SparkSyntax(mockExplorer);
            SparkSyntaxTypes syntaxType;
            sparkSyntax.IsSparkSyntax(' ', out syntaxType);

            mockExplorer.VerifyAllExpectations();
            Assert.That(syntaxType, Is.Not.EqualTo(SparkSyntaxTypes.Attribute));
        }
    }
}
