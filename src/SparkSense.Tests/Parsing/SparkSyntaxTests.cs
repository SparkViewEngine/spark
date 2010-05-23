using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using SparkSense.Parsing;

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

            Assert.That(syntaxType, Is.EqualTo(SparkSyntaxTypes.Tag)); 
        }

        [Test]
        public void ShouldTriggerAtributeCompletionWhenInsideATag()
        {
            var mockExplorer = MockRepository.GenerateMock<ITextExplorer>();

            mockExplorer.Expect(x => x.IsCaretContainedWithinTag()).Return(true);

            var sparkSyntax = new SparkSyntax(mockExplorer);
            SparkSyntaxTypes syntaxType;
            sparkSyntax.IsSparkSyntax(' ', out syntaxType);

            Assert.That(syntaxType, Is.EqualTo(SparkSyntaxTypes.Attribute));
        }

        [Test]
        public void ShouldNotTriggerAtributeCompletionWhenOutsideATag()
        {
            var mockExplorer = MockRepository.GenerateMock<ITextExplorer>();
            
            mockExplorer.Expect(x => x.IsCaretContainedWithinTag()).Return(false);
            
            var sparkSyntax = new SparkSyntax(mockExplorer);
            SparkSyntaxTypes syntaxType;
            sparkSyntax.IsSparkSyntax(' ', out syntaxType);

            mockExplorer.VerifyAllExpectations();
            Assert.That(syntaxType, Is.Not.EqualTo(SparkSyntaxTypes.Attribute));
        }
    }
}
