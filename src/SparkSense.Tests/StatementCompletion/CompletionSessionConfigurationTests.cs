using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using SparkSense.Parsing;
using System.Collections.Generic;
using SparkSense.StatementCompletion;

namespace SparkSense.Tests.StatementCompletion
{
    [TestFixture]
    public class CompletionSessionConfigurationTests
    {

        [Test]
        public void ConfigurationShouldCreateASessionFromCompletionBroker()
        {
            var mockBroker = MockRepository.GenerateMock<ICompletionBroker>();
            var mockTextExplorer = MockRepository.GenerateMock<ITextExplorer>();
            var stubTextView = MockRepository.GenerateStub<ITextView>();
            var stubTrackingPoint = MockRepository.GenerateStub<ITrackingPoint>();
            var stubSession = MockRepository.GenerateStub<ICompletionSession>();

            mockTextExplorer.Expect(x => x.TextView).Return(stubTextView);
            mockTextExplorer.Expect(x => x.GetTrackingPoint()).Return(stubTrackingPoint);
            mockBroker.Expect(x => x.CreateCompletionSession(stubTextView, stubTrackingPoint, true)).Return(stubSession);

            var config = new CompletionSessionConfiguration(mockBroker);

            ICompletionSession session;
            Assert.That(config.TryCreateCompletionSession(mockTextExplorer, out session));
            Assert.That(session, Is.EqualTo(stubSession));

            mockTextExplorer.VerifyAllExpectations();
            mockBroker.VerifyAllExpectations();
        }

        [Test]
        public void ConfigurationShouldAddPropertiesRequiredForCompletionSourceToSession()
        {
            var mockSession = MockRepository.GenerateMock<ICompletionSession>();
            var stubBroker = MockRepository.GenerateMock<ICompletionBroker>();
            var stubTextExplorer = MockRepository.GenerateMock<ITextExplorer>();

            var propertyTrackingSpan = MockRepository.GenerateStub<ITrackingSpan>();
            var propertyViewVolder = MockRepository.GenerateStub<IViewExplorer>();
            var propertySyntaxType = SparkSyntaxTypes.Tag;

            var mockProperties = new PropertyCollection();
            var properties = new List<object> { propertyTrackingSpan, propertyViewVolder, propertySyntaxType };

            stubBroker.Stub(x => x.CreateCompletionSession(null, null, true)).IgnoreArguments().Return(mockSession);

            mockSession.Expect(x => x.Properties).Return(mockProperties).Repeat.Times(3);

            var config = new CompletionSessionConfiguration(stubBroker);
            ICompletionSession session;
            config.TryCreateCompletionSession(stubTextExplorer, out session);
            config.AddCompletionSourceProperties(properties);

            Assert.That(mockProperties.ContainsProperty(propertyTrackingSpan.GetType()));
            Assert.That(mockProperties.ContainsProperty(propertyViewVolder.GetType()));
            Assert.That(mockProperties.ContainsProperty(typeof(SparkSyntaxTypes)));
            mockSession.VerifyAllExpectations();
        }
    }
}
